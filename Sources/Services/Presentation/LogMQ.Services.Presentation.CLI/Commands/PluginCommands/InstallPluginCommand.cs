using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.PluginCommands;

/// <summary>
/// Command to install or update a plugin from a .lmqex file.
/// Handles plugin installation, version management, and optional automatic enabling.
/// </summary>
/// <remarks>
/// This command supports:
/// - Installing new plugins
/// - Updating existing plugins
/// - Reinstalling specific versions
/// - Automatic plugin enabling after installation
/// - Version conflict resolution
/// - Automatic confirmation for batch operations
/// </remarks>
/// <param name="manager">The plugin manager instance used to handle plugin operations.</param>
public class InstallPluginCommand(IPluginManager manager) : AsyncCommand<InstallPluginCommand.Settings>
{
	/// <summary>
	/// Settings class that defines the command-line arguments and options for the plugin installation command.
	/// </summary>
	public class Settings : CommandSettings
	{
		/// <summary>
		/// The file system path to the plugin package file (.lmqex).
		/// </summary>
		[CommandArgument(0, "<PLUGIN_PATH>")]
		[Description("The path of the .lmqex plugin file.")]
		public string PluginPath { get; set; }

		/// <summary>
		/// When true, the plugin will be enabled immediately after installation.
		/// </summary>
		[CommandOption("-e|--enable")]
		[Description("Enable the plugin immediately.")]
		public bool Enable { get; set; }

		/// <summary>
		/// When true, automatically restarts the broker after installation.
		/// </summary>
		[CommandOption("-r|--restart")]
		[Description("Automatically restarts the broker after install.")]
		public bool Restart { get; set; }

		/// <summary>
		/// When true, skips all confirmation prompts with automatic 'yes' responses.
		/// </summary>
		[CommandOption("-y")]
		[Description("Automatically respond y to all prompts.")]
		public bool Yes { get; set; }
	}

	/// <summary>
	/// Executes the plugin installation command asynchronously.
	/// </summary>
	/// <param name="context">The command execution context.</param>
	/// <param name="settings">The command settings containing installation options and plugin path.</param>
	/// <returns>
	/// Returns 0 if the installation was successful, -1 if the operation was cancelled or failed.
	/// The command will:
	/// - Analyze the plugin package
	/// - Check for version conflicts
	/// - Handle existing installations
	/// - Perform the installation/update
	/// - Display the updated plugin status
	/// </returns>
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var manifest = await manager.AnalyzePluginFile(settings.PluginPath);
		var existingPlugin = await manager.GetPluginInfo(PluginConfigType.Staged, manifest.Id.ToString());

		if (settings.Yes)
		{
			bool blockExecution = false;
			string infoMessage = "[green]Installing plugin[/]";
			if (existingPlugin != null)
			{
				if (existingPlugin.Versions.Any(x => x.Version == manifest.Version))
				{
					infoMessage = $"[red]Error: The plugin '{manifest.Name}' version '{manifest.Version}' you are attempting to install already exists. Run this command again without the -y option to reinstall.[/]";
					blockExecution = true;
				}
				else if (existingPlugin.Versions.Any(x => x.Version > manifest.Version))
				{
					var newerVersion = existingPlugin.Versions.Where(x => x.Version > manifest.Version).Max(x => x.Version);
					infoMessage = $"[red]Error: A newer version '{newerVersion}' of the plugin '{manifest.Name}' is already installed. Run this command again without the -y option to install version '{manifest.Version}'.[/]";
					blockExecution = true;
				}
				else
				{
					var olderVersion = existingPlugin.Versions.Max(x => x.Version);
					infoMessage = $"[green]Updating plugin '{manifest.Name}': {olderVersion} -> {manifest.Version}[/]";
				}
			}
			AnsiConsole.MarkupLine(infoMessage);
			if (blockExecution) return -1;
		}
		else
		{
			string promptMessage = $"Do you want to confirm the installation of the plugin '{manifest.Name}' version '{manifest.Version}'?";
			if (existingPlugin != null)
			{
				if (existingPlugin.Versions.Any(x => x.Version == manifest.Version))
				{
					promptMessage = $"The plugin '{manifest.Name}' version '{manifest.Version}' you are attempting to install already exists. Do you want to reinstall it?";
				}
				else if (existingPlugin.Versions.Any(x => x.Version > manifest.Version))
				{
					var newerVersion = existingPlugin.Versions.Where(x => x.Version > manifest.Version).Max(x => x.Version);
					promptMessage = $"A newer version '{newerVersion}' of the plugin '{manifest.Name}' is already installed. Do you want to install the older version '{manifest.Version}'?";
				}
				else
				{
					var olderVersion = existingPlugin.Versions.Max(x => x.Version);
					promptMessage = $"Do you want to update the plugin '{manifest.Name}' from version '{olderVersion}' to '{manifest.Version}'?";
				}
			}
			if (!ConfirmOperation(settings.Yes, promptMessage))
			{
				return -1;
			}
		}

		var plugin = await manager.InstallPluginAsync(settings.PluginPath, true, settings.Enable, manifest);

		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' version '{manifest.Version}' installed successfully![/]");
		AnsiConsole.WriteLine();

		//TODO: Implement restart

		var runningPlugin = await manager.GetRunningVersion(plugin.Id);
		ShowPluginTree(plugin, runningPlugin);

		return 0;
	}
}

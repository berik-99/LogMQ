using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.PluginCommands;
/// <summary>
/// Command to enable a specified version of a plugin.
/// Handles plugin enabling, version management, and optional automatic broker restart.
/// </summary>
/// <remarks>
/// This command supports:
/// - Enabling specific versions of a plugin
/// - Handling multiple versions through user prompts
/// - Automatic confirmation for batch operations
/// - Optional broker restart after enabling the plugin
/// </remarks>
/// <param name="manager">The plugin manager instance used to handle plugin operations.</param>
public class EnablePluginCommand(IPluginManager manager) : AsyncCommand<EnablePluginCommand.Settings>
{
	/// <summary>
	/// Settings class that defines the command-line arguments and options for the plugin enabling command.
	/// </summary>
	public class Settings : CommandSettings
	{
		/// <summary>
		/// The identifier used to locate a plugin. Can be either:
		/// - Plugin ID (GUID)
		/// - Plugin Name
		/// </summary>
		[CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
		[Description("Specify the plugin by its ID (GUID) or name.")]
		public string PluginIdOrName { get; set; }

		/// <summary>
		/// The specific version of the plugin to enable.
		/// </summary>
		[CommandOption("-v|--version <VERSION>")]
		[Description("Specify the version of the plugin to enable.")]
		public Version Version { get; set; }

		/// <summary>
		/// When true, automatically restarts the broker after the operation.
		/// </summary>
		[CommandOption("-r|--restart")]
		[Description("Automatically restarts the broker after operation.")]
		public bool Restart { get; set; }

		/// <summary>
		/// When true, skips all confirmation prompts with automatic 'yes' responses.
		/// </summary>
		[CommandOption("-y")]
		[Description("Automatically respond y to all prompts.")]
		public bool Yes { get; set; }
	}

	/// <summary>
	/// Executes the plugin enabling command asynchronously.
	/// </summary>
	/// <param name="context">The command execution context.</param>
	/// <param name="settings">The command settings containing enabling options and plugin identifier.</param>
	/// <returns>
	/// Returns 0 if the enabling was successful, -1 if the operation was cancelled or failed.
	/// The command will:
	/// - Retrieve plugin information based on the provided identifier
	/// - Handle version selection and confirmation prompts
	/// - Perform the enabling of the specified version
	/// - Display the updated plugin status
	/// </returns>
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

		if (plugin == null)
		{
			AnsiConsole.MarkupLine($"[red]Error: Plugin '{settings.PluginIdOrName}' not found.[/]");
			return -1;
		}

		if (settings.Version != null)
		{
			var version = plugin.Versions.FirstOrDefault(x => x.Version == settings.Version);
			if (version == null)
			{
				AnsiConsole.MarkupLine($"[red]Error: Version '{settings.Version}' not found for plugin '{plugin.Name}'.[/]");
				return -1;
			}

			if (version.Status == VersionStatus.Removed && !ConfirmOperation(settings.Yes, $"You are attempting to enable a removed version '{settings.Version}' of plugin '{plugin.Name}'. Do you want to proceed and undo the removal?"))
				return -1;
		}
		else
		{
			settings.Version = plugin.Versions.Count > 1 ? AnsiConsole.Prompt(new SelectionPrompt<Version>()
					.Title("Select the version which you want to uninstall:")
					.AddChoices(plugin.Versions.Select(x => x.Version))) : plugin.Versions.First().Version;
		}
		var currentActive = plugin.Versions.FirstOrDefault(x => x.Status == VersionStatus.Enabled)?.Version;
		if (currentActive != null)
		{
			if (currentActive > settings.Version && !ConfirmOperation(settings.Yes, $"You are attempting to enable an older version '{settings.Version}' than the current enabled version '{currentActive}' of plugin '{plugin.Name}'. Do you want to proceed?"))
				return -1;
			else if (currentActive < settings.Version)
				settings.Version = plugin.Versions.Max(x => x.Version);
		}

		if (!ConfirmOperation(settings.Yes, $"Are you sure you want to enable the plugin '{plugin.Name}' version '{settings.Version}'?"))
			return -1;

		plugin = await manager.EnablePluginAsync(plugin.Id, settings.Version);

		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name} v{settings.Version}' enabled successfully![/]");
		AnsiConsole.WriteLine();

		//TODO: Implement restart

		var runningPlugin = await manager.GetRunningVersion(plugin.Id);
		ShowPluginTree(plugin, runningPlugin);

		return 0;
	}
}

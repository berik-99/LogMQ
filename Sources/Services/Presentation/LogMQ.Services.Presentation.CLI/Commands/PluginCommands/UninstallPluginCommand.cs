using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.PluginCommands;

/// <summary>
/// Command to uninstall specified versions of a plugin.
/// Handles plugin uninstallation, version management, and optional automatic broker restart.
/// </summary>
/// <remarks>
/// This command supports:
/// - Uninstalling specific versions of a plugin
/// - Handling multiple versions through user prompts
/// - Automatic confirmation for batch operations
/// - Optional broker restart after the uninstallation
/// </remarks>
/// <param name="manager">The plugin manager instance used to handle plugin operations.</param>
public class UninstallPluginCommand(IPluginManager manager) : AsyncCommand<UninstallPluginCommand.Settings>
{
	/// <summary>
	/// Settings class that defines the command-line arguments and options for the plugin uninstallation command.
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
		/// The specific version of the plugin to uninstall.
		/// </summary>
		[CommandOption("-v|--version <VERSION>")]
		[Description("Specify the version of the plugin to uninstall.")]
		public Version Version { get; set; }

		/// <summary>
		/// When true, automatically restarts the broker after the uninstallation.
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
	/// Executes the plugin uninstallation command asynchronously.
	/// </summary>
	/// <param name="context">The command execution context.</param>
	/// <param name="settings">The command settings containing uninstallation options and plugin identifier.</param>
	/// <returns>
	/// Returns 0 if the uninstallation was successful, -1 if the operation was cancelled or failed.
	/// The command will:
	/// - Retrieve plugin information based on the provided identifier
	/// - Handle version selection and confirmation prompts
	/// - Perform the uninstallation of specified versions
	/// - Display the updated plugin status
	/// </returns>
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		List<Version> versions = [];

		var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

		if (plugin == null)
		{
			AnsiConsole.MarkupLine($"[red]Error: Plugin '{settings.PluginIdOrName}' not found.[/]");
			return -1;
		}

		if (settings.Version != null)
		{
			if (!plugin.Versions.Any(x => x.Version == settings.Version))
			{
				AnsiConsole.MarkupLine($"[red]Error: Version '{settings.Version}' not found for plugin '{plugin.Name}'.[/]");
				return -1;
			}
			versions.Add(settings.Version);
		}
		else if (plugin.Versions.Count == 1)
		{
			versions.Add(plugin.Versions.First().Version);
		}
		else
		{
			versions = AnsiConsole.Prompt(new MultiSelectionPrompt<Version>()
				.Title("Select the version which you want to uninstall: (at least one is required)")
				.InstructionsText("[grey](Press [blue]<space>[/] to toggle a version, [green]<enter>[/] to confirm)[/]")
				.AddChoices(plugin.Versions.Select(x => x.Version)));
		}

		string versionsText = string.Join(", ", versions);

		if (!ConfirmOperation(settings.Yes, $"You are attempting to uninstall the plugin '{plugin.Name}' version(s) '{versionsText}'. Do you want to proceed?"))
			return -1;

		plugin = await manager.UninstallPluginAsync(plugin.Id, versions);

		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' version(s) '{versionsText}' uninstalled successfully![/]");
		AnsiConsole.WriteLine();

		//TODO: Implement restart

		var runningPlugin = await manager.GetRunningVersion(plugin.Id);
		ShowPluginTree(plugin, runningPlugin);

		return 0;
	}
}
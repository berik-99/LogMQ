using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.PluginCommands;

/// <summary>
/// Command to disable a specified plugin.
/// Handles plugin disabling and optional automatic broker restart.
/// </summary>
/// <remarks>
/// This command supports:
/// - Disabling a plugin based on its ID or name
/// - Automatic confirmation for batch operations
/// - Optional broker restart after disabling the plugin
/// </remarks>
/// <param name="manager">The plugin manager instance used to handle plugin operations.</param>
public class DisablePluginCommand(IPluginManager manager) : AsyncCommand<DisablePluginCommand.Settings>
{
	/// <summary>
	/// Settings class that defines the command-line arguments and options for the plugin disabling command.
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
		/// When true, automatically restarts the broker after the operation.
		/// </summary>
		[CommandOption("-r|--restart")]
		[Description("Automatically restarts the broker after the operation.")]
		public bool Restart { get; set; }

		/// <summary>
		/// When true, skips all confirmation prompts with automatic 'yes' responses.
		/// </summary>
		[CommandOption("-y")]
		[Description("Automatically respond 'yes' to all prompts.")]
		public bool Yes { get; set; }
	}

	/// <summary>
	/// Executes the plugin disabling command asynchronously.
	/// </summary>
	/// <param name="context">The command execution context.</param>
	/// <param name="settings">The command settings containing disabling options and plugin identifier.</param>
	/// <returns>
	/// Returns 0 if the disabling was successful, -1 if the operation was cancelled or failed.
	/// The command will:
	/// - Retrieve plugin information based on the provided identifier
	/// - Prompt for confirmation if the '-y' option is not set
	/// - Perform the disabling of the specified plugin
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

		// Prompt for confirmation if -y option is not set
		if (!ConfirmOperation(settings.Yes, $"You are attempting to disable the plugin '{plugin.Name}'. Do you want to proceed?"))
		{
			return -1;
		}

		plugin = await manager.DisablePluginAsync(plugin.Id);

		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' disabled successfully![/]");
		AnsiConsole.WriteLine();

		//TODO: Implement restart

		var runningPlugin = await manager.GetRunningVersion(plugin.Id);
		ShowPluginTree(plugin, runningPlugin);

		return 0;
	}
}

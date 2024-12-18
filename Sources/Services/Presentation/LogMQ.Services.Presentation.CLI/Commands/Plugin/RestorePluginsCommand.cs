using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

/// <summary>
/// Command to restore the plugin configuration.
/// Handles restoring the configuration with options to erase new installed plugins.
/// </summary>
/// <remarks>
/// This command supports:
/// - Restoring the plugin configuration to a previous state
/// - Optionally making a hard copy of the running configuration and removing newly installed plugins
/// - Automatic confirmation for batch operations
/// </remarks>
/// <param name="manager">The plugin manager instance used to handle plugin operations.</param>
public class RestorePluginsCommand(IPluginManager manager) : AsyncCommand<RestorePluginsCommand.Settings>
{
    /// <summary>
    /// Settings class that defines the command-line arguments and options for the plugin restore command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        /// When true, makes a hard copy of the running configuration and removes newly installed plugins.
        /// </summary>
        [CommandOption("-H|--hard-copy")]
        [Description("Will make a hard copy of running config, removing also new installed plugins.")]
        public bool Erase { get; set; }

        /// <summary>
        /// When true, skips all confirmation prompts with automatic 'yes' responses.
        /// </summary>
        [CommandOption("-y")]
        [Description("Automatically respond y to all prompts.")]
        public bool Yes { get; set; }
    }

    /// <summary>
    /// Executes the plugin restore command asynchronously.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="settings">The command settings containing restore options.</param>
    /// <returns>
    /// Returns 0 if the restore operation was successful, -1 if the operation was cancelled or failed.
    /// The command will:
    /// - Confirm the irreversible operation with the user
    /// - Restore the plugin configuration based on the provided settings
    /// - Display the restored plugin list with their current running versions
    /// </returns>
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!ConfirmOperation(settings.Yes, "This operation is not reversible. Do you want to continue?"))
        {
            return -1;
        }

        var plugins = await manager.RestorePluginConfigAsync(settings.Erase);

        Dictionary<PluginConfig, Version> dict = [];
        foreach (var plugin in plugins)
            dict.Add(plugin, await manager.GetRunningVersion(plugin.Id));

        AnsiConsole.WriteLine();
        if (settings.Erase)
            AnsiConsole.MarkupLine("[green]Success: The plugin configuration has been restored and newly installed plugins have been removed![/]");
        else
            AnsiConsole.MarkupLine("[green]Success: The plugin configuration has been restored![/]");
        AnsiConsole.WriteLine();

        ShowPluginListTree(dict);
        return 0;
    }
}
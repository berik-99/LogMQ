using System.ComponentModel;
using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console.Cli;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.PluginCommands;

/// <summary>
/// Command to display detailed information about a plugin.
/// This command can retrieve information from either a plugin file or an installed plugin.
/// </summary>
/// <remarks>
/// The command supports:
/// - Analyzing plugin files to display their manifest information
/// - Retrieving information about installed plugins using their ID or name
/// - Showing the current running version of the plugin
/// - Displaying plugin details including versions, status, and metadata
/// </remarks>
/// <param name="manager">The plugin manager instance used to interact with plugins.</param>
public class InfoCommand(IPluginManager manager) : AsyncCommand<InfoCommand.Settings>
{
    /// <summary>
    /// Settings class that defines the command-line arguments and options for the plugin info command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        /// The identifier used to locate a plugin. Can be either:
        /// - Plugin ID (GUID)
        /// - Plugin Name
        /// - Path to plugin file
        /// </summary>
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME_OR_PATH>")]
        [Description("Specify the plugin by its ID (GUID), name, or file path.")]
        public string PluginIdentifier { get; set; }

        /// <summary>
        /// Determines which configuration environment to use for plugin operations.
        /// </summary>
        [CommandOption("-c|--config-type")]
        [Description("Specify the configuration type to use (Staged or Running). Defaults to Staged.")]
        public PluginConfigType ConfigType { get; set; } = PluginConfigType.Staged;
    }

    /// <summary>
    /// Executes the plugin info command asynchronously.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="settings">The command settings containing the plugin identifier and configuration type.</param>
    /// <returns>
    /// Returns 0 if the command executed successfully.
    /// The command will:
    /// - Attempt to analyze the plugin file if a file path is provided
    /// - Retrieve plugin information from the specified configuration
    /// - Display the plugin information including versions and running status
    /// </returns>
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        PluginManifest manifest;
        try
        {
            manifest = await manager.AnalyzePluginFile(settings.PluginIdentifier);
        }
        catch (Exception)
        {
            manifest = null;
        }
        string pluginId = settings.PluginIdentifier;
        if (manifest != null) pluginId = manifest.Id.ToString();

        PluginConfig plugin = await manager.GetPluginInfo(settings.ConfigType, pluginId);

        List<Version> otherVerison = null;

        if (manifest != null)
        {
            otherVerison = [manifest.Version];
            plugin ??= new PluginConfig
            {
                Id = manifest.Id,
                Name = manifest.Name,
                Author = manifest.Author,
                Description = manifest.Description,
                Type = manifest.Type,
                EntryPoint = manifest.EntryPoint,
                Versions = [new() { Version = manifest.Version }]
            };
        }

        Version runningPlugin = await manager.GetRunningVersion(plugin.Id);
        ShowPluginTree(plugin, runningPlugin, otherVerison);

        return 0;
    }
}
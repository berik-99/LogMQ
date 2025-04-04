using System.ComponentModel;
using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console.Cli;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.PluginCommands;

/// <summary>
/// Command to list plugins based on configuration type and plugin type.
/// </summary>
/// <remarks>
/// This command supports:
/// - Listing plugins from a specific configuration (Staged or Running)
/// - Filtering plugins by type (Receiver, Storage, etc.)
/// - Displaying the list of plugins with their current running versions
/// </remarks>
/// <param name="manager">The plugin manager instance used to handle plugin operations.</param>
public class ListCommand(IPluginManager manager) : AsyncCommand<ListCommand.Settings>
{
    /// <summary>
    /// Settings class that defines the command-line arguments and options for the plugin listing command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        /// The configuration type to list plugins from.
        /// Defaults to Staged if not specified.
        /// </summary>
        [CommandOption("-c|--config-type")]
        [Description("Show plugins from specific configuration (Defaults to Staged).")]
        public PluginConfigType ConfigType { get; set; } = PluginConfigType.Staged;

        /// <summary>
        /// The type of plugins to list.
        /// If not specified, lists both Receiver and Storage plugins.
        /// </summary>
        [CommandOption("-t|--type")]
        [Description("Show plugins of the specified type.")]
        public PluginType? Type { get; set; }
    }

    /// <summary>
    /// Executes the plugin listing command asynchronously.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="settings">The command settings containing listing options.</param>
    /// <returns>
    /// Returns 0 if the listing was successful.
    /// The command will:
    /// - Retrieve a list of plugins based on the configuration type and type filter
    /// - Display the plugins with their current running versions
    /// </returns>
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        List<PluginType> typefilter = [];
        if (settings.Type == null)
            typefilter.AddRange([PluginType.Receiver, PluginType.Storage]);
        else
            typefilter.Add((PluginType)settings.Type);
        List<PluginConfig> plugins = await manager.ListPluginsAsync(settings.ConfigType, typefilter);

        Dictionary<PluginConfig, Version> dict = [];
        foreach (PluginConfig plugin in plugins)
            dict.Add(plugin, await manager.GetRunningVersion(plugin.Id));

        ShowPluginListTree(dict);
        return 0;
    }
}

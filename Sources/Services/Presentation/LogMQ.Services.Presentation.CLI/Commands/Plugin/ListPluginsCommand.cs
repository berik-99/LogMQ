using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class ListPluginsCommand(IPluginManager manager) : AsyncCommand<ListPluginsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-c|--config-type")]
        [Description("Show plugins from specific configuration (Defaults to Staged).")]
        public PluginConfigType ConfigType { get; set; } = PluginConfigType.Staged;

        [CommandOption("-t|--type")]
        [Description("Show plugins of the specified type.")]
        public PluginType? Type { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        List<PluginType> typefilter = [];
        if (settings.Type == null)
            typefilter.AddRange([PluginType.Receiver, PluginType.Storage]);
        else
            typefilter.Add((PluginType)settings.Type);
        var plugins = await manager.ListPluginsAsync(settings.ConfigType, typefilter);

        Dictionary<PluginConfig, Version> dict = [];
        foreach (var plugin in plugins)
            dict.Add(plugin, await manager.GetRunningVersion(plugin.Id));

        ShowPluginListTree(dict);
        return 0;
    }
}

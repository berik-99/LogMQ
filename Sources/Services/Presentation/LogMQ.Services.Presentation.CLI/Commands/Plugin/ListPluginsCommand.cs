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
        var plugins = await manager.ListPluginsAsync(settings.ConfigType, settings.Type);
        ShowPluginListTree(plugins);
        return 0;
    }
}

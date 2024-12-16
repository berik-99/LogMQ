using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class ListPluginsCommand(IPluginManager manager) : AsyncCommand<ListPluginsCommand.Settings>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

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

        if (plugins.Count > 0)
        {
            var tree = CommonCommands.BuildPluginListTree(plugins);
            AnsiConsole.Write(tree);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No plugins found.[/]");
        }
        return 0;
    }
}

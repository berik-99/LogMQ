using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class RestorePluginsCommand(IPluginManager manager) : AsyncCommand<RestorePluginsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-H|--hard-copy")]
        [Description("Will make an hard copy of running config, removing also new installed plugins.")]
        public bool Erase { get; set; }

        [CommandOption("-y")]
        [Description("Automatically respond y to all propmts.")]
        public bool Yes { get; set; }
    }

    //TODO: implement keep-installed mode which will keep the installed plugins and only restore the configuration of active/inactive plugins
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

        ShowPluginListTree(dict);
        return 0;
    }
}
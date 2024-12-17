using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class PluginInfoCommand(IPluginManager manager) : AsyncCommand<PluginInfoCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME_OR_PATH>")]
        public string PluginIdentifier { get; set; }

        [CommandOption("-c|--config-type")]
        [Description("Show plugins from specific configuration (Defaults to Staged).")]
        public PluginConfigType ConfigType { get; set; } = PluginConfigType.Staged;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var manifest = await manager.AnalyzePluginFile(settings.PluginIdentifier);
        string pluginId = settings.PluginIdentifier;
        if (manifest != null) pluginId = manifest.Id.ToString();

        var plugin = await manager.GetPluginInfo(settings.ConfigType, pluginId);

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

        var runningPlugin = await manager.GetRunningVersion(plugin.Id);
        ShowPluginTree(plugin, runningPlugin, otherVerison);

        return 0;
    }
}
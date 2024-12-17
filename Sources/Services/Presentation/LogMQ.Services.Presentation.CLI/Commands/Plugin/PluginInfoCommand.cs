using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class PluginInfoCommand(IPluginManager manager) : AsyncCommand<PluginInfoCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME_OR_PATH>")]
        public string PluginIdentifier { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        //TODO: when is plugin file parse with installed versions and merge informations
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdentifier);
        if (plugin != null)
        {
            var tree = CommonCommands.BuildPluginTree(plugin);
            AnsiConsole.Write(tree);
            return 0;
        }

        var manifest = await manager.AnalyzePluginFile(settings.PluginIdentifier);
        if (manifest != null)
        {
            var tree = CommonCommands.BuildPluginTree(manifest);
            AnsiConsole.Write(tree);
            return 0;
        }

        AnsiConsole.MarkupLine($"[red]Plugin '{settings.PluginIdentifier}' not found.[/]");
        return -1;
    }
}
using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class UninstallPluginCommand(IPluginManager manager) : AsyncCommand<UninstallPluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
        public string PluginIdOrName { get; set; }

        [CommandOption("-v|--version <VERSION>")]
        public Version Version { get; set; }

        [CommandOption("-r|--restart")]
        public bool Restart { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        if (!plugin.Versions.Contains(settings.Version))
        {
            AnsiConsole.MarkupLine($"[red]Version '{settings.Version}' not found for plugin '{plugin.Name}'[/]");
            return -1;
        }

        settings.Version ??= plugin.EnabledVersion;

        plugin = await manager.UninstallPluginAsync(plugin.Id, settings.Version);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' uninstalled successfully![/]");
        AnsiConsole.WriteLine();

        var pluginTree = CommonCommands.BuildPluginTree(plugin, removed: [settings.Version]);

        AnsiConsole.Write(pluginTree);

        return 0;
    }
}
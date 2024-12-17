using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

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
        [Description("Automatically restarts the broker after operation.")]
        public bool Restart { get; set; }

        [CommandOption("-y")]
        [Description("Automatically respond y to all propmts.")]
        public bool Yes { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        //TODO: Implement restart
        //TODO: Implement -all flag to uninstall all versions 
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        if (!plugin.Versions.Exists(x => x.Version == settings.Version))
        {
            AnsiConsole.MarkupLine($"[red]Error: Version '{settings.Version}' not found for plugin '{plugin.Name}'.[/]");
            return -1;
        }

        settings.Version ??= plugin.CurrentVersion;

        if (!ConfirmOperation(settings.Yes, $"You are attempting to uninstall the plugin '{plugin.Name}' version '{settings.Version}'. Do you want to proceed?"))
        {
            return -1;
        }

        plugin = await manager.UninstallPluginAsync(plugin.Id, settings.Version);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' version '{settings.Version}' uninstalled successfully![/]");
        AnsiConsole.WriteLine();

        ShowPluginTree(plugin);
        return 0;
    }
}
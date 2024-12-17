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

    //TODO: Implement restart
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        List<Version> versions = [];

        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        if (settings.Version != null)
        {
            if (!plugin.Versions.Exists(x => x.Version == settings.Version))
            {
                AnsiConsole.MarkupLine($"[red]Error: Version '{settings.Version}' not found for plugin '{plugin.Name}'.[/]");
                return -1;
            }
            versions.Add(settings.Version);
        }
        else if (plugin.Versions.Count == 1)
        {
            versions.Add(plugin.Versions.First().Version);
        }
        else
        {
            versions = AnsiConsole.Prompt(new MultiSelectionPrompt<Version>()
                .Title("Select the version which you want to uninstall: (at least one is required)")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a version, [green]<enter>[/] to confirm)[/]")
                .AddChoices(plugin.Versions.Select(x => x.Version)));
        }

        string versionsText = string.Join(", ", versions);

        if (!ConfirmOperation(settings.Yes, $"You are attempting to uninstall the plugin '{plugin.Name}' version(s) '{versionsText}'. Do you want to proceed?"))
            return -1;

        plugin = await manager.UninstallPluginAsync(plugin.Id, versions);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' version(s) '{versionsText}' uninstalled successfully![/]");
        AnsiConsole.WriteLine();

        var runningPlugin = await manager.GetRunningVersion(plugin.Id);
        ShowPluginTree(plugin, runningPlugin);
        return 0;
    }
}
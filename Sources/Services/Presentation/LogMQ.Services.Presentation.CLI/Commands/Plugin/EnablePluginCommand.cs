using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class EnablePluginCommand(IPluginManager manager) : AsyncCommand<EnablePluginCommand.Settings>
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
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        if (settings.Version != null)
        {
            var version = plugin.Versions.FirstOrDefault(x => x.Version == settings.Version);
            if (version == null)
            {
                AnsiConsole.MarkupLine($"[red]Error: Version '{settings.Version}' not found for plugin '{plugin.Name}'.[/]");
                return -1;
            }

            if (version.Status == VersionStatus.Removed && !ConfirmOperation(settings.Yes, $"You are attempting to enable a removed version '{settings.Version}' of plugin '{plugin.Name}'. Do you want to proceed and undo the removal?"))
                return -1;
        }
        else
        {
            settings.Version = plugin.Versions.Count > 1 ? AnsiConsole.Prompt(new SelectionPrompt<Version>()
                    .Title("Select the version which you want to uninstall:")
                    .AddChoices(plugin.Versions.Select(x => x.Version))) : plugin.Versions.First().Version;
        }
        var currentActive = plugin.Versions.FirstOrDefault(x => x.Status == VersionStatus.Enabled)?.Version;
        if (currentActive != null)
        {
            if (currentActive > settings.Version && !ConfirmOperation(settings.Yes, $"You are attempting to enable an older version '{settings.Version}' than the current enabled version '{currentActive}' of plugin '{plugin.Name}'. Do you want to proceed?"))
                return -1;
            else if (currentActive < settings.Version)
                settings.Version = plugin.Versions.Max(x => x.Version);
        }

        if (!ConfirmOperation(settings.Yes, $"Are you sure you want to enable the plugin '{plugin.Name}' version '{settings.Version}'?"))
            return -1;

        plugin = await manager.EnablePluginAsync(plugin.Id, settings.Version);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name} v{currentActive}' enabled successfully![/]");
        AnsiConsole.WriteLine();

        var runningPlugin = await manager.GetRunningVersion(plugin.Id);
        ShowPluginTree(plugin, runningPlugin);

        return 0;
    }
}

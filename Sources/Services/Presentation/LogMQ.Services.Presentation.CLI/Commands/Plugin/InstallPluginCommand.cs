using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class InstallPluginCommand(IPluginManager manager) : AsyncCommand<InstallPluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_PATH>")]
        [Description("The path of the .lmqex plugin file.")]
        public string PluginPath { get; set; }

        [CommandOption("-e|--enable")]
        [Description("Enable the plugin immediatelly.")]
        public bool Enable { get; set; }

        [CommandOption("-r|--restart")]
        [Description("Automatically restarts the broker after install.")]
        public bool Restart { get; set; }

        [CommandOption("-y")]
        [Description("Automatically respond y to all propmts.")]
        public bool Yes { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var manifest = await manager.AnalyzePluginFile(settings.PluginPath);
        var existingPlugin = await manager.GetPluginInfo(PluginConfigType.Staged, manifest.Id.ToString());

        if (settings.Yes)
        {
            bool blockExecution = false;
            string infoMessage = "[green]Installing plugin[/]";
            if (existingPlugin != null)
            {
                if (existingPlugin.Versions.Exists(x => x.Version == manifest.Version))
                {
                    infoMessage = $"[red]Error: The plugin '{manifest.Name}' version '{manifest.Version}' you are attempting to install already exists. Run this command again without the -y option to reinstall.[/]";
                    blockExecution = true;
                }
                else if (existingPlugin.Versions.Exists(x => x.Version > manifest.Version))
                {
                    var newerVersion = existingPlugin.Versions.Where(x => x.Version > manifest.Version).Max(x => x.Version);
                    infoMessage = $"[red]Error: A newer version '{newerVersion}' of the plugin '{manifest.Name}' is already installed. Run this command again without the -y option to install version '{manifest.Version}'.[/]";
                    blockExecution = true;
                }
                else
                {
                    var olderVersion = existingPlugin.Versions.Max(x => x.Version);
                    infoMessage = $"[green]Updating plugin '{manifest.Name}': {olderVersion} -> {manifest.Version}[/]";
                }
            }
            AnsiConsole.MarkupLine(infoMessage);
            if (blockExecution) return -1;
        }
        else
        {
            string promptMessage = $"Do you want to confirm the installation of the plugin '{manifest.Name}' version '{manifest.Version}'?";
            if (existingPlugin != null)
            {
                if (existingPlugin.Versions.Exists(x => x.Version == manifest.Version))
                {
                    promptMessage = $"The plugin '{manifest.Name}' version '{manifest.Version}' you are attempting to install already exists. Do you want to reinstall it?";
                }
                else if (existingPlugin.Versions.Exists(x => x.Version > manifest.Version))
                {
                    var newerVersion = existingPlugin.Versions.Where(x => x.Version > manifest.Version).Max(x => x.Version);
                    promptMessage = $"A newer version '{newerVersion}' of the plugin '{manifest.Name}' is already installed. Do you want to install the older version '{manifest.Version}'?";
                }
                else
                {
                    var olderVersion = existingPlugin.Versions.Max(x => x.Version);
                    promptMessage = $"Do you want to update the plugin '{manifest.Name}' from version '{olderVersion}' to '{manifest.Version}'?";
                }
            }
            if (!ConfirmOperation(settings.Yes, promptMessage))
            {
                return -1;
            }
        }

        var plugin = await manager.InstallPluginAsync(settings.PluginPath, true, settings.Enable, manifest);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' version '{manifest.Version}' installed successfully![/]");
        AnsiConsole.WriteLine();

        //TODO: Implement restart

        var runningPlugin = await manager.GetRunningVersion(plugin.Id);
        ShowPluginTree(plugin, runningPlugin);

        return 0;
    }
}

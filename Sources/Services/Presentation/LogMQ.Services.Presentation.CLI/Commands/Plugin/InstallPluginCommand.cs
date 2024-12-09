using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

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
                if (existingPlugin.Versions.Contains(manifest.Version))
                {
                    infoMessage = "[red]The plugin version you are trying to install already exists. Run again this command without -y option to reinstall.[/]";
                    blockExecution = true;
                }
                else if (existingPlugin.Versions.Any(x => x > manifest.Version))
                {
                    infoMessage = "[red]A newer version of this plugin is already installed. Run again this command without -y option to to install this version.[/]";
                    blockExecution = true;
                }
                else
                {
                    infoMessage = $"[green]Updating plugin: {existingPlugin.Versions.Max()} -> {manifest.Version}[/]";
                }
            }
            AnsiConsole.MarkupLine(infoMessage);
            if (blockExecution) return -1;
        }
        else
        {
            string promptMessage = "Do you want to confirm the installation of this plugin?";
            if (existingPlugin != null)
            {
                if (existingPlugin.Versions.Contains(manifest.Version))
                {
                    promptMessage = "The plugin version you are trying to install already exists. Do you want to reinstall it?";
                }
                else if (existingPlugin.Versions.Any(x => x > manifest.Version))
                {
                    promptMessage = "A newer version of this plugin is already installed. Do you want to install this older version?";
                }
                else
                {
                    promptMessage = "An older version of this plugin is already installed. Do you want to update?";
                }
            }

            var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>(promptMessage)
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(true)
            .WithConverter(choice => choice ? "y" : "n"));
            if (!confirmation)
            {
                AnsiConsole.MarkupLine("[red]Installation aborted.[/]");
                return -1;
            }
        }

        var plugin = await manager.InstallPluginAsync(settings.PluginPath, true, settings.Enable, manifest);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' installed successfully![/]");
        AnsiConsole.WriteLine();

        var pluginTree = CommonCommands.BuildPluginTree(plugin, added: [manifest.Version]);

        AnsiConsole.Write(pluginTree);

        //TODO: Implement the restart logic

        return 0;
    }
}

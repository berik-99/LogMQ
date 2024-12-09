using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class DisablePluginCommand(IPluginManager manager) : AsyncCommand<DisablePluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
        public string PluginIdOrName { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }


        var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>("[yellow]You are about to disable this plugin. Do you want to proceed?[/]")
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(true)
            .WithConverter(choice => choice ? "y" : "n"));
        if (!confirmation)
        {
            AnsiConsole.MarkupLine("[red]Operation aborted.[/]");
            return -1;
        }

        plugin.EnabledVersion = null;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' installed successfully![/]");
        AnsiConsole.WriteLine();

        var pluginTree = CommonCommands.BuildPluginTree(plugin);

        AnsiConsole.Write(pluginTree);

        return 0;
    }
}
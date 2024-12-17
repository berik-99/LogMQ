using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class DisablePluginCommand(IPluginManager manager) : AsyncCommand<DisablePluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
        public string PluginIdOrName { get; set; }

        [CommandOption("-r|--restart")]
        [Description("Automatically restarts the broker after the operation.")]
        public bool Restart { get; set; }

        [CommandOption("-y")]
        [Description("Automatically respond 'yes' to all prompts.")]
        public bool Yes { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        //TODO: implement -y option
        var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>("You are about to disable this plugin. Do you want to proceed?")
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(true)
            .WithConverter(choice => choice ? "y" : "n"));
        if (!confirmation)
        {
            AnsiConsole.MarkupLine("[red]Operation aborted.[/]");
            return -1;
        }

        plugin = await manager.DisablePluginAsync(plugin.Id);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' disabled successfully![/]");
        AnsiConsole.WriteLine();

        var pluginTree = CommonCommands.BuildPluginTree(plugin);

        AnsiConsole.Write(pluginTree);

        return 0;
    }
}

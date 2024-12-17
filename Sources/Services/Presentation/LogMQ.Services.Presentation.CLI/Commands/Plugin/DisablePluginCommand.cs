using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

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
        //TODO: Implement restart
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        // Prompt for confirmation if -y option is not set
        if (!ConfirmOperation(settings.Yes, $"You are attempting to disable the plugin '{plugin.Name}'. Do you want to proceed?"))
        {
            return -1;
        }

        plugin = await manager.DisablePluginAsync(plugin.Id);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Success: Plugin '{plugin.Name}' disabled successfully![/]");
        AnsiConsole.WriteLine();

        ShowPluginTree(plugin);

        return 0;
    }
}

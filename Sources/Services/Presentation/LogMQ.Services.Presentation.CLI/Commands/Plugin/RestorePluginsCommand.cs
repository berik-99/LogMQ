using LogMQ.Services.Shared.PluginManager;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class RestorePluginsCommand(IPluginManager manager) : AsyncCommand<RestorePluginsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-y")]
        [Description("Automatically respond y to all propmts.")]
        public bool Yes { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!settings.Yes)
        {

            var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>("This operation is not reversible. Do you want to continue?")
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
        var plugins = await manager.RestorePluginConfigAsync();

        if (plugins.Count > 0)
        {
            var tree = CommonCommands.BuildPluginListTree(plugins);
            AnsiConsole.Write(tree);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No plugins found.[/]");
        }
        return 0;
    }
}
using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class EnablePluginCommand(IPluginManager manager) : AsyncCommand<EnablePluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
        public string PluginIdOrName { get; set; }

        [CommandOption("-v|--version <VERSION>")]
        public Version Version { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var plugin = await manager.GetPluginInfo(PluginConfigType.Staged, settings.PluginIdOrName);

        if (plugin == null)
        {
            AnsiConsole.MarkupLine($"[red]Plugin '{settings.PluginIdOrName}' not found.[/]");
            return -1;
        }

        if (settings.Version != null)
        {
            if (!plugin.Versions.Contains(settings.Version))
            {
                AnsiConsole.MarkupLine($"[red]Version '{settings.Version}' not found for plugin '{plugin.Name}'[/]");
                return -1;
            }

            if (plugin.EnabledVersion > settings.Version)
            {
                var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>("[yellow]You are trying to enable an older version than current enabled. Do you want to proceed?[/]")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(choice => choice ? "y" : "n"));
                if (!confirmation)
                {
                    AnsiConsole.MarkupLine("[red]Operation aborted.[/]");
                    return -1;
                }
            }
        }
        else
        {
            settings.Version = plugin.Versions.Max();
        }

        plugin = await manager.EnablePluginAsync(plugin.Id, settings.Version);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' installed successfully![/]");
        AnsiConsole.WriteLine();

        var pluginTree = CommonCommands.BuildPluginTree(plugin);

        AnsiConsole.Write(pluginTree);

        return 0;
    }
}
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class InstallPluginCommand : Command<InstallPluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_PATH>")]
        public string PluginPath { get; set; }

        [CommandOption("-r|--restart")]
        public bool Restart { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.PluginPath))
        {
            AnsiConsole.Markup("[red]Error:[/] Plugin file not found.\n");
            return -1;
        }

        AnsiConsole.Markup($"[green]Installing plugin from:[/] {settings.PluginPath}\n");

        if (settings.Restart)
        {
            AnsiConsole.Markup("[yellow]Force flag detected. Restarting broker after installation.[/]\n");
            // Logic to restart broker
        }

        return 0;
    }
}

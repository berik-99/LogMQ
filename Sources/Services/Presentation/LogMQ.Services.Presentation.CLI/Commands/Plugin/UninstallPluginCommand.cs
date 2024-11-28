using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class UninstallPluginCommand : Command<UninstallPluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
        public string PluginIdOrName { get; set; }

        [CommandOption("-v|--version <VERSION>")]
        public string Version { get; set; }

        [CommandOption("-r|--restart")]
        public bool Restart { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.Markup($"[red]Uninstalling plugin:[/] {settings.PluginIdOrName}\n");

        if (settings.Restart)
        {
            AnsiConsole.Markup("[yellow]Force flag detected. Restarting broker after uninstallation.[/]\n");
            // Logic to restart broker
        }

        // Logic to uninstall the plugin
        return 0;
    }
}
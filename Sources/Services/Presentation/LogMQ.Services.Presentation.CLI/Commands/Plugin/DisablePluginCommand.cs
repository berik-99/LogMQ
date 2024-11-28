using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class DisablePluginCommand : Command<DisablePluginCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PLUGIN_ID_OR_NAME>")]
        public string PluginIdOrName { get; set; }

        [CommandOption("-v|--version <VERSION>")]
        public string Version { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.Markup($"[yellow]Disabling plugin:[/] {settings.PluginIdOrName}\n");

        if (!string.IsNullOrWhiteSpace(settings.Version))
        {
            AnsiConsole.Markup($"[yellow]Version:[/] {settings.Version}\n");
        }

        // Logic to disable the plugin
        return 0;
    }
}
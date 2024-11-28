using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class EnablePluginCommand : Command<EnablePluginCommand.Settings>
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
        AnsiConsole.Markup($"[green]Enabling plugin:[/] {settings.PluginIdOrName}\n");

        if (!string.IsNullOrWhiteSpace(settings.Version))
        {
            AnsiConsole.Markup($"[green]Version:[/] {settings.Version}\n");
        }

        // Logic to enable the plugin
        return 0;
    }
}
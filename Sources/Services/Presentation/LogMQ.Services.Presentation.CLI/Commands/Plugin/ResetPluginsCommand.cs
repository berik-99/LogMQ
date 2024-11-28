using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class ResetPluginsCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.Markup("[yellow]Resetting plugin configuration...[/]\n");

        // Reset configuration logic here
        AnsiConsole.Markup("[green]Plugin configuration reset completed.[/]\n");

        return 0;
    }
}
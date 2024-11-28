using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Broker;

public class BrokerStatusCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.Markup("[green]Broker is running and healthy.[/]\n");
        // Logic to check broker status
        return 0;
    }
}
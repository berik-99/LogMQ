using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.BrokerCommands;

public class StopBrokerCommand : Command
{
	public override int Execute(CommandContext context)
	{
		AnsiConsole.Markup("[yellow]Stopping the LogMQ broker...[/]\n");
		// Logic to restart broker
		return 0;
	}
}

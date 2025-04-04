using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.BrokerCommands;

public class RestartCommand : Command
{
	public override int Execute(CommandContext context)
	{
		AnsiConsole.Markup("[yellow]Restarting the LogMQ broker...[/]\n");
		// Logic to restart broker
		return 0;
	}
}
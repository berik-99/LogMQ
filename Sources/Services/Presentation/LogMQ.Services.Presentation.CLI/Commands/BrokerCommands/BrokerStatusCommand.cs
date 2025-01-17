using LogMQ.Services.Shared.BrokerManager;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.BrokerCommands;

public class BrokerStatusCommand(IBrokerManager manager) : AsyncCommand
{
	public override async Task<int> ExecuteAsync(CommandContext context)
	{
        string str = await manager.GetBrokerStatusAsync();
		AnsiConsole.WriteLine(str);
		return 0;
	}
}
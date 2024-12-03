using LogMQ.Services.Shared.PluginManager;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Broker;

public class BrokerStatusCommand(IBrokerManager manager) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var str = await manager.GetBrokerStatusAsync();
        AnsiConsole.WriteLine(str);
        return 0;
    }
}
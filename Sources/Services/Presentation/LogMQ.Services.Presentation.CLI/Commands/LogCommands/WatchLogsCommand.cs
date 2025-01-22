using Grpc.Core;
using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.LogCommands;

public class WatchLogsCommand : AsyncCommand<WatchLogsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<APP_NAME_OR_ID>")]
        public string ApplicationName { get; set; }

        [CommandOption("-i|--interval")]
        public int Interval { get; set; }

        [CommandOption("--broker")]
        public string GrpcAddress { get; set; } = Shared.Common.Defaults.GrpcAddress;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            LogManager manager = new(settings.GrpcAddress);
            Guid lastId = Guid.Empty;
            while (true)
            {
                DateTime now = DateTime.Now;
                LogFilter filter = new() { ApplicationName = settings.ApplicationName, DateFrom = now.AddSeconds(-5), DateTo = now, Count = int.MaxValue };
                List<LogMessage> newlogs = await manager.GetLogsAsync(filter);
                foreach (LogMessage log in newlogs)
                {
                    if (log.Guid != lastId)
                    {
                        AnsiConsole.MarkupLine($"[grey]{log.Timestamp}[/] [yellow]{log.LogLevel}[/] [aqua]{log.Application.Name}[/] {log.Message}");
                        lastId = log.Guid;
                    }
                }
                await Task.Delay(settings.Interval);
            }
        }
        catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
        {
            AnsiConsole.Markup("[red]Error: Cannot connect to broker api service.[/]");
            return -1;
        }
    }
}

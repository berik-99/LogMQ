using Grpc.Core;
using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.LogCommands;

public class WatchLogsCommand(ILogManager manager) : AsyncCommand<WatchLogsCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<APP_NAME_OR_ID>")]
		public string ApplicationName { get; set; }

		[CommandOption("-i|--interval")]
		public int Interval { get; set; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		try
		{
			var lastId = Guid.Empty;
			while (true)
			{
				var logs = await manager.GetLastLogsAsync("http://localhost:5000", settings.ApplicationName, lastId, 1);
				lastId = logs.LastOrDefault()?.Guid ?? lastId;
				foreach (var message in logs)
				{
					AnsiConsole.MarkupLine($"[bold]Message:[/] {message.Message}");
				}
				await Task.Delay(settings.Interval);
			}
		}
		catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
		{
			AnsiConsole.Markup("[red]Error: Cannot connect to broker api service.[/]");
		}
		return 0;
	}
}

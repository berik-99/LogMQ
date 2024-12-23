using Grpc.Core;
using LogMQ.Services.Shared.LogManager;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.LogCommands;

public class ShowLogsCommand(ILogManager manager) : AsyncCommand<ShowLogsCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<APP_NAME_OR_ID>")]
		public string ApplicationName { get; set; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		try
		{
			var logs = await manager.GetLogsByFilterAsync("http://localhost:5000", settings.ApplicationName);

			foreach (var message in logs)
			{
				AnsiConsole.MarkupLine($"[bold]Message:[/] {message.Message}");
			}
		}
		catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
		{
			AnsiConsole.Markup("[red]Error: Cannot connect to broker api service.[/]");
		}
		return 0;
	}
}

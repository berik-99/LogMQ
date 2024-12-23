using Grpc.Net.Client;
using LogMQ.Services.Presentation.GrpcContracts;
using ProtoBuf.Grpc.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.Log;

public class ShowLogsCommand : AsyncCommand<ShowLogsCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<APP_NAME_OR_ID>")]
		public string ApplicationName { get; set; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		using var channel = GrpcChannel.ForAddress("http://localhost:5000");
		var client = channel.CreateGrpcService<ILogService>();

		var reply = await client.ShowLogAsync(new ShowLogRequest { ApplicationName = settings.ApplicationName });

		foreach (var message in reply.MessageList)
		{
			AnsiConsole.MarkupLine($"[bold]Message:[/] {message.Message}");
		}
		return 0;
	}
}

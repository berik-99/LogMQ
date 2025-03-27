using Grpc.Core;
using LogMQ.Services.Shared.LogManager;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.Commands.LogCommands;

public class ListAppsCommand : AsyncCommand<ListAppsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--broker")]
        public string GrpcAddress { get; set; } = Defaults.GrpcAddress;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            LogManager manager = new(settings.GrpcAddress);
            List<string> applicationNames = await manager.GetLogApplications();
            if (applicationNames?.Count > 0)
            {
                AnsiConsole.Markup("[bold underline green]Applications:[/]\n");
                Table table = new();
                table.AddColumn("Name");
                applicationNames.ForEach(name => table.AddRow(name));
                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.Markup("[red]No applications found.[/]");
            }

            return 0;
        }
        catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
        {
            AnsiConsole.Markup("[red]Error: Cannot connect to broker api service.[/]");
            return -1;
        }
    }
}




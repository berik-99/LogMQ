using System.Globalization;
using Grpc.Core;
using LogMQ.Core;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Services.Shared.LogManager.Filters;
using Spectre.Console;
using Spectre.Console.Cli;
using Constants = LogMQ.Services.Shared.LogManager.Constants;

namespace LogMQ.Services.Presentation.CLI.Commands.LogCommands;

public class ShowCommand : AsyncCommand<ShowCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<APP_NAME_OR_ID>")]
        public string ApplicationName { get; set; }

        [CommandOption("--count")]
        public ulong? Count { get; set; }

        [CommandOption("--range")]
        public string Range { get; set; }

        [CommandOption("--broker")]
        public string GrpcAddress { get; set; } = Constants.GrpcAddress;

        [CommandOption("--type")]
        public LogLevel? Type { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            LogManager manager = new(settings.GrpcAddress);
            UniversalDateTime dateTo = UniversalDateTime.Now;
            UniversalDateTime dateFrom = UniversalDateTime.MinValue;

            if (!string.IsNullOrEmpty(settings.Range))
            {
                string[] parts = settings.Range.Split('|');
                if (parts.Length > 1)
                {
                    dateTo = ParseDate(parts[1], dateTo, false);
                }
                if (parts.Length > 0)
                {
                    dateFrom = ParseDate(parts[0], dateTo, true);
                }
            }
            else if (settings.Count != null)
            {
                AnsiConsole.MarkupLine("[red]Error: You must specify at least a time range (`--range`) or a count (`--count`).[/]");
                return -1;
            }

            if (dateFrom >= dateTo)
            {
                AnsiConsole.MarkupLine("[red]Error: The start date must be earlier than the end date.[/]");
                return -1;
            }

            AnsiConsole.MarkupLine($"Fetching {settings.Count?.ToString() ?? "all"} logs from {dateFrom:yyyy/MM/dd HH:mm:ss.fff} to {dateTo:yyyy/MM/dd HH:mm:ss.fff}");
            SearchFilter filter = new() { ApplicationName = settings.ApplicationName, DateFrom = dateFrom, DateTo = dateTo, Count = settings.Count, LogLevel = settings.Type };
            ulong count = await manager.CountLogsAsync(filter);
            if (count > 10000)
            {
                if (!CommonCommands.ConfirmOperation(false, $"[yellow]The search yielded {count} results. Printing these results may take several minutes. " +
                    "Press y to continue, otherwise try again with a shorter time interval.[/]"))
                {
                    return -1;
                }
            }
            List<LogMessage> logs = await manager.GetLogsAsync(filter);
            Table table = new();
            table.Expand();

            table.AddColumn("Timestamp");
            table.AddColumn("LogLevel");
            table.AddColumn("Message");
            foreach (LogMessage log in logs)
            {
                string timestamp = $"[grey]{log.Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff zzz")}[/]";

                string logLevel = log.LogLevel switch
                {
                    LogLevel.Trace => "[dim]Trace[/]",
                    LogLevel.Debug => "[blue]Debug[/]",
                    LogLevel.Information => "[green]Information[/]",
                    LogLevel.Warning => "[yellow]Warning[/]",
                    LogLevel.Error => "[red]Error[/]",
                    LogLevel.Critical => "[bold red]Critical[/]",
                    _ => log.LogLevel.ToString()
                };

                table.AddRow(timestamp, logLevel, log.Message);
            }
            AnsiConsole.Write(table);
            return 0;
        }
        catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
        {
            AnsiConsole.Markup("[red]Error: Cannot connect to broker api service.[/]");
            return -1;
        }
    }

    private static UniversalDateTime ParseDate(string date, UniversalDateTime referenceDate, bool isDateFrom)
    {
        if (DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
        {
            return isDateFrom ? dateTime : dateTime.AddSeconds(1).AddTicks(-1);
        }
        if (DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            return isDateFrom ? dateTime : dateTime.AddMinutes(1).AddTicks(-1);
        }
        if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            return isDateFrom ? dateTime : dateTime.AddDays(1).AddTicks(-1);
        }
        if (DateTime.TryParseExact(date, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            dateTime = DateTime.Today.Add(dateTime.TimeOfDay);
            return isDateFrom ? dateTime : dateTime.AddSeconds(1).AddTicks(-1);
        }
        if (DateTime.TryParseExact(date, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            dateTime = DateTime.Today.Add(dateTime.TimeOfDay);
            return isDateFrom ? dateTime : dateTime.AddMinutes(1).AddTicks(-1);
        }

        return isDateFrom
            ? ParseQuickFilter(date, referenceDate)
            : throw new ArgumentException("Invalid date format. Only full date time allowed for the `date-to` parameter.");
    }

    private static UniversalDateTime ParseQuickFilter(string value, UniversalDateTime referenceDate)
    {
        if (value.Length < 2) throw new ArgumentException("Invalid string format. Please use a full date time or quick date time.");
        char unit = value[^1];
        if (!int.TryParse(value[..^1], out int amount))
        {
            throw new ArgumentException("Invalid number format in the quick filter.");
        }

        return unit switch
        {
            's' => referenceDate.AddSeconds(-amount),
            'm' => referenceDate.AddMinutes(-amount),
            'h' => referenceDate.AddHours(-amount),
            'd' => referenceDate.AddDays(-amount),
            'w' => referenceDate.AddDays(-amount * 7),
            'M' => referenceDate.AddMonths(-amount),
            'y' => referenceDate.AddYears(-amount),
            _ => throw new ArgumentException("Invalid time range unit. Use 's' for seconds, 'm' for minutes, 'h' for hours, 'd' for days, 'w' for weeks, 'M' for months, or 'y' for years.")
        };
    }
}
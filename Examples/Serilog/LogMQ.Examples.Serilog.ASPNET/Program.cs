using LogMQ.Loggers.Serilog;
using LogMQ.Providers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
   .ClearProviders()
   .AddSerilog(
        new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .UseLogMQStack(new TcpProvider(null, "localhost", 5563, new FallbackLogger()), "ConsoleApplication", "TCP_PROVIDER")
            .WriteTo.Console()
            .CreateLogger()
   );

builder.Services.AddSingleton<ExampleHandler>();

var app = builder.Build();

var handler = app.Services.GetRequiredService<ExampleHandler>();
app.MapGet("/", handler.HandleRequest);

await app.RunAsync();

partial class ExampleHandler(ILogger<ExampleHandler> logger)
{
    public string HandleRequest()
    {
        LogHandleRequest(logger);
        return "Hello World";
    }

    [LoggerMessage(LogLevel.Information, "ExampleHandler.HandleRequest was called")]
    public static partial void LogHandleRequest(Microsoft.Extensions.Logging.ILogger logger);
}
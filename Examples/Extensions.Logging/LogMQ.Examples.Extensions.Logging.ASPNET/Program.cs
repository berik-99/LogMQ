using LogMQ.Extensions.Logging;
using LogMQ.Providers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddLogMQ(new TcpProvider(null, "localhost", 5563, new FallbackLogger()), "MS-EXT-LOGGING");

builder.Services.AddSingleton<ExampleHandler>();

WebApplication app = builder.Build();

ExampleHandler handler = app.Services.GetRequiredService<ExampleHandler>();
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
	public static partial void LogHandleRequest(ILogger logger);
}
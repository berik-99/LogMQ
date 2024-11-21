using LogMQ.Extensions.Logging;
using LogMQ.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var provider = new TcpProvider(null, "localhost", 5563, null);

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging(loggingBuilder =>
{
	loggingBuilder.ClearProviders(); // Rimuove i logger predefiniti
	loggingBuilder.AddLogMQ(provider, "MyApplicationName");
	loggingBuilder.SetMinimumLevel(LogLevel.Information);
});

var serviceProvider = serviceCollection.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Questo è un messaggio di log inviato a LogMQ!");
logger.LogError(new Exception("Test Exception"), "Si è verificato un errore.");
Console.WriteLine("Log inviato con successo!");
Console.ReadLine();

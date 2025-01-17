using LogMQ.Extensions.Logging;
using LogMQ.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace LogMQ.Examples.Extensions.Logging.Console;

public class Program
{
	private static ILogger<Program> logger;

	private static async Task Main(string[] args)
	{
		Timer timer = new(1000)
		{
			Enabled = false,
			AutoReset = true
		};
		timer.Elapsed += (sender, e) => logger.LogInformation("This is a test log message from Timer");

        ServiceCollection serviceCollection = new();
		serviceCollection.AddLogging(loggingBuilder =>
		{
			loggingBuilder.ClearProviders();
			loggingBuilder.AddLogMQ(new TcpProvider(null, "localhost", 5563, new FallbackLogger()), "MS-EXT-LOGGING");
			loggingBuilder.AddConsole();
			loggingBuilder.SetMinimumLevel(LogLevel.Information);
		});
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		logger = serviceProvider.GetRequiredService<ILogger<Program>>();

		logger.LogInformation("This is a test log message from Main");
		TestSyncMethod(23, 8646);
		await TestAsyncMethod("ciao", false);
        Action<int> m = (int arg) => logger.LogInformation("This is a test log message from lambda sync method. args: {Arg}", arg);
        Func<int, Task> m2 = async (int arg) =>
		{
			await Task.Delay(1);
			logger.LogInformation("This is a test log message from lambda async method. args: {Arg}", arg);
		};

		m(1);
		await m2(2);
		logger.LogError(new Exception("Sample exception"), "An error occurred");

		timer.Start();
		System.Console.ReadLine();
	}

	private static void TestSyncMethod(int arg1, int arg2)
	{
		logger.LogInformation("This is a test log message from internal sync method. args: {Arg1} {Arg2}", arg1, arg2);
	}

	private static async Task TestAsyncMethod(string arg1, bool arg2)
	{
		await Task.Delay(1);
		logger.LogInformation("This is a test log message from internal async method. args: {Arg1} {Arg2}", arg1, arg2);
	}
}

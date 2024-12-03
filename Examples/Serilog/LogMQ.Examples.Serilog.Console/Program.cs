using LogMQ.Loggers.Serilog;
using LogMQ.Providers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Timer = System.Timers.Timer;

namespace LogMQ.Examples.Serilog.Console;

public static class Program
{
	private static async Task Main(string[] args)
	{
		Timer timer = new(1000)
		{
			Enabled = false,
			AutoReset = true
		};
		timer.Elapsed += (sender, e) => Log.Information("This is a test log message from Timer");
		//.WriteTo.Async(x => x.LogMQ(new MsmqProvider(null, @".\Private$\LogMQ_Queue", new FallbackLogger()), "ConsoleApplication", "MSMQ_PROVIDER"))
		//.Enrich.WithLogMQMetadata()
		//.WriteTo.Async(x => x.LogMQ(new TcpProvider(null, "localhost", 5563, new FallbackLogger()), "ConsoleApplication", "TCP_PROVIDER", LogEventLevel.Fatal, new LoggingLevelSwitch(LogEventLevel.Information)))

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			//.UseLogMQStack(new TcpProvider(null, "localhost", 5563, new FallbackLogger()), "ConsoleApplication", "TCP_PROVIDER", LogEventLevel.Fatal, new LoggingLevelSwitch(LogEventLevel.Information))
			.Enrich.WithLogMQMetadata()
			.WriteTo.Async(x => x.LogMQ(new TcpProvider(null, "localhost", 5563, new FallbackLogger(), 10), "ConsoleApplication", "TCP_PROVIDER", LogEventLevel.Fatal, new LoggingLevelSwitch(LogEventLevel.Information)))
			.WriteTo.Console()
			.CreateLogger();

		Log.Information("This is a test log message from Main");
		TestSyncMethod(23, 8646);
		await TestAsyncMethod("ciao", false);
		var m = (int arg) => Log.Information("This is a test log message from lambda sync method. args: {Arg}", arg);
		var m2 = async (int arg) =>
		{
			await Task.Delay(1);
			Log.Information("This is a test log message from lambda async method. args: {Arg}", arg);
		};

		m(1);
		await m2(2);
		Log.Error(new Exception("Sample exception"), "An error occurred");

		timer.Start();

		//long count = 0;
		//while (count < long.MaxValue)
		//{
		//    Log.Information("This is the #{Count} message from loop", count);
		//    count++;
		//    await Task.Delay(1000);
		//}
		System.Console.ReadLine();
	}

	private static void TestSyncMethod(int arg1, int arg2)
	{
		Log.Information("This is a test log message from internal sync method. args: {Arg1} {Arg2}", arg1, arg2);
	}

	private static async Task TestAsyncMethod(string arg1, bool arg2)
	{
		await Task.Delay(1);
		Log.Information("This is a test log message from internal async method. args: {Arg1} {Arg2}", arg1, arg2);
	}
}
using LogMQ.Loggers.Serilog;
using LogMQ.Loggers.Serilog.Extensions;
using LogMQ.Providers;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace LogMQ.Examples.Serilog.Console;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Async(x => x.LogMQ(new TcpProvider(null, "localhost", 5563, new FallbackLogger()), "ConsoleApplication", "TCP_PROVIDER", LogEventLevel.Fatal, new LoggingLevelSwitch(LogEventLevel.Information)))
            //.WriteTo.Async(x => x.LogMQ(new MsmqProvider(null, @".\Private$\LogMQ_Queue", new FallbackLogger()), "ConsoleApplication", "MSMQ_PROVIDER"))
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("This is a test log message from Main");
        TestSyncMethod(23, 8646);
        await TestAsyncMethod("ciao", false);
        var m = (int arg) => Log.Information("This is a test log message from lambda sync method");
        var m2 = async (int arg) => Log.Information("This is a test log message from lambda async method");

        m(1);
        await m2(2);
        Log.Error(new Exception("Sample exception"), "An error occurred");

        long count = 0;
        while (true)
        {
            Log.Verbose("This is the #{Count} VERBOSE message from loop", count);
            Log.Debug("This is the #{Count} DEBUG message from loop", count);
            Log.Information("This is the #{Count} INFO message from loop", count);
            Log.Warning("This is the #{Count} WARNING message from loop", count);
            Log.Error("This is the #{Count} ERROR message from loop", count);
            Log.Fatal("This is the #{Count} FATAL message from loop", count);
            System.Console.WriteLine($"Sent log #{count}");
            count++;
            await Task.Delay(250);
        }
    }

    private static void TestSyncMethod(int arg1, int arg2)
    {
        Log.Information("This is a test log message from internal sync method");
    }

    private static async Task TestAsyncMethod(string arg1, bool arg2)
    {
        await Task.Delay(1);
        Log.Information("This is a test log message from internal async method");
    }
}
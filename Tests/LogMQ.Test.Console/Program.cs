using Serilog;
using Serilog.Sinks.LogMQ;

namespace LogMQ.Test.Console;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.LogMQBrokerSink(applicationName: "BrokerSink")
            .WriteTo.LogMQMSMQSink(applicationName: "MSMQSink")
            .CreateLogger();

        Log.Information("This is a test log message from Main");
        TestSyncMethod(23, 8646);
        await TestAsyncMethod("ciao", false);
        var m = (int arg) => Log.Information("This is a test log message from lambda sync method");
        var m2 = async (int arg) => Log.Information("This is a test log message from lambda async method");

        m(1);
        await m2(2);
        Log.Error(new Exception("Sample exception"), "An error occurred");

        await Task.Delay(10000);

        Log.CloseAndFlush();

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
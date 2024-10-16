//using LogMQ;

//MQLogger.LoadConfiguration();
//MQLogger.Log("Initializing");

//var t1 = Task.Run(() =>
//{
//    for (int i = 0; i < 1000; i++)
//    {
//        MQLogger.Log($"t1 index {i}");
//    }
//});

//var t2 = Task.Run(() =>
//{
//    for (int i = 0; i < 1000; i++)
//    {
//        MQLogger.Log($"t2 index {i}");
//    }
//});

//var t3 = Task.Run(() =>
//{
//    for (int i = 0; i < 1000; i++)
//    {
//        MQLogger.Log($"t3 index {i}");
//    }
//});

//var t4 = Task.Run(() =>
//{
//    for (int i = 0; i < 1000; i++)
//    {
//        MQLogger.Log($"t4 index {i}");
//    }
//});

//await Task.WhenAll(t1, t2, t3, t4);

//MQLogger.Log("End");

//Console.ReadKey();



using Serilog;
using Serilog.Sinks.LogMQ;


internal class Program
{
	private static async Task Main(string[] args)
	{
		const string host = "127.0.0.1";
		const int port = 5563;

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.WriteTo.LogMQBrokerSink(host, port)
			.WriteTo.LogMQMSMQSink()
			.CreateLogger();

		Log.Information("This is a test log message from Main");
		TestSyncMethod(23, 8646);
		await TestAsyncMethod("ciao", false);
		var m = (int arg) => Log.Information("This is a test log message from lambda sync method");
		var m2 = async (int arg) => Log.Information("This is a test log message from lambda async method");

		m(1);
		await m2(2);
		Log.Error(new Exception("Sample exception"), "An error occurred");

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
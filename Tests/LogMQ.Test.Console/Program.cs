using LogMQ;

MQLogger.LoadConfiguration();
MQLogger.Log("Initializing");

var t1 = Task.Run(() =>
{
    for (int i = 0; i < 1000; i++)
    {
        MQLogger.Log($"t1 index {i}");
    }
});

var t2 = Task.Run(() =>
{
    for (int i = 0; i < 1000; i++)
    {
        MQLogger.Log($"t2 index {i}");
    }
});

var t3 = Task.Run(() =>
{
    for (int i = 0; i < 1000; i++)
    {
        MQLogger.Log($"t3 index {i}");
    }
});

var t4 = Task.Run(() =>
{
    for (int i = 0; i < 1000; i++)
    {
        MQLogger.Log($"t4 index {i}");
    }
});

await Task.WhenAll(t1, t2, t3, t4);

MQLogger.Log("End");

Console.ReadKey();
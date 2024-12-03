namespace LogMQ.Services.Shared.PluginManager;

public static class BrokerManager
{
#if Windows
    public const string BrokerSerivePlatform = "Windows";
    public const string BrokerSeriveName = "LogMQ Broker";
    public const string ViewerSeriveName = "LogMQ Viewer";
#elif Linux
    public const string BrokerSerivePlatform = "Linux";
    public const string BrokerSeriveName = "logmq-broker";
    public const string BrokerSeriveName = "logmq-viewer";
#elif MacOS
    public const string BrokerSerivePlatform = "MacOS";
    public const string BrokerSeriveName = "logmq-broker";
    public const string BrokerSeriveName = "logmq-viewer";
#endif

    public static async Task<string> GetBrokerStatusAsync()
    {
        await Task.Delay(1);
        return BrokerSeriveName;
    }

    public static async Task RestartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public static async Task StartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public static async Task StopBrokerAsync()
    {
        throw new NotImplementedException();
    }
}

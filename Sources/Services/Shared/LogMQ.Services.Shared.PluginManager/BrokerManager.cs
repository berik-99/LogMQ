namespace LogMQ.Services.Shared.PluginManager;

public interface IBrokerManager
{
    public string BrokerSerivePlatform { get; }
    public string BrokerSeriveName { get; }
    public string ViewerSeriveName { get; }

    public Task<string> GetBrokerStatusAsync();

    public Task RestartBrokerAsync();

    public Task StartBrokerAsync();

    public Task StopBrokerAsync();
}

public class WindowsBrokerManager : IBrokerManager
{
    public string BrokerSerivePlatform => "Windows";
    public string BrokerSeriveName => "LogMQ Broker";
    public string ViewerSeriveName => "LogMQ Viewer";

    public async Task<string> GetBrokerStatusAsync()
    {
        await Task.Delay(1);
        return $"{BrokerSerivePlatform}_{BrokerSeriveName}";
    }

    public async Task RestartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public async Task StartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public async Task StopBrokerAsync()
    {
        throw new NotImplementedException();
    }
}

public class LinuxBrokerManager : IBrokerManager
{
    public string BrokerSerivePlatform => "Linux";
    public string BrokerSeriveName => "logmq-broker";
    public string ViewerSeriveName => "logmq-viewer";

    public async Task<string> GetBrokerStatusAsync()
    {
        await Task.Delay(1);
        return $"{BrokerSerivePlatform}_{BrokerSeriveName}";
    }

    public async Task RestartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public async Task StartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public async Task StopBrokerAsync()
    {
        throw new NotImplementedException();
    }
}

namespace LogMQ.Services.Shared.BrokerManager;

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

    public async Task RestartBrokerAsync() => throw new NotImplementedException();

    public async Task StartBrokerAsync() => throw new NotImplementedException();

    public async Task StopBrokerAsync() => throw new NotImplementedException();
}

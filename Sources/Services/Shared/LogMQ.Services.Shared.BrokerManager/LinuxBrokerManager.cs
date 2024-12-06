namespace LogMQ.Services.Shared.BrokerManager;

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

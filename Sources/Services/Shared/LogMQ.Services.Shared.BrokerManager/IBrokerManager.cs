namespace LogMQ.Services.Shared.BrokerManager;

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

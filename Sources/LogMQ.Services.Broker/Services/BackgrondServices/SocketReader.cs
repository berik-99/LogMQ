using LogMQ.Broker.Services.InternalQueueServices;
using System.Text;
using WatsonTcp;

namespace LogMQ.Broker.Services.BackgrondServices;

public class SocketReader(ILogger<SocketReader> logger, RocksDbService rdb) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Init Socket Reader");
        using WatsonTcpServer tcpServer = new("localhost", 5563);
        tcpServer.Events.MessageReceived += async (s, e) => await MessageReceived(e);
        tcpServer.Callbacks.SyncRequestReceivedAsync = SyncMessageReceived;
        tcpServer.Start();
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);
        }
    }

    private Task<SyncResponse> SyncMessageReceived(SyncRequest request)
    {
        byte[] message = Encoding.ASCII.GetBytes("PONG1");
        string ping = Encoding.UTF8.GetString(request.Data);
        if (ping != "PING")
            throw new InvalidOperationException("Invalid ping message from client");
        return Task.FromResult(new SyncResponse(request, message));
    }


    private async Task MessageReceived(MessageReceivedEventArgs e)
    {
        Console.WriteLine("received");
        await using MemoryStream stream = new(e.Data);
        var logMessage = LogMessage.Deserialize(stream);
        await rdb.WriteLogMessage(logMessage);
    }
}

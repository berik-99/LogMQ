using LogMQ.Broker.Services.InternalQueueServices;
using MSMQ.Messaging;

namespace LogMQ.Broker.Services.BackgrondServices;

public class MSMQReader(ILogger<MSMQReader> logger, RocksDbService rdb) : BackgroundService
{
    private readonly string queuePath = @".\Private$\LogMQ_Queue";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Init MSMQ Reader");
        if (!MessageQueue.Exists(queuePath))
            MessageQueue.Create(queuePath);
        using MessageQueue queue = new(queuePath);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Run(async () =>
            {
                Message message = queue.Receive();
                var stream = message.BodyStream;
                var logMessage = LogMessage.Deserialize(stream);
                await rdb.WriteLogMessage(logMessage);
            }, stoppingToken);
        }
    }
}

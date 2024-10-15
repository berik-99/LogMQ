using System.Text;
using WatsonTcp;

namespace LogMQ.Broker
{
    public class Worker : BackgroundService
    {
        private readonly WatsonTcpServer server;
        public Worker()
        {
            try
            {
                server = new("localhost", 9000);
                server.Events.ClientConnected += ClientConnected;
                server.Events.ClientDisconnected += ClientDisconnected;
                server.Events.MessageReceived += MessageReceived;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            server.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }

        private void ClientConnected(object sender, ConnectionEventArgs args)
        {
            Console.WriteLine($"Client connected: {args.Client}");
        }

        private void ClientDisconnected(object sender, DisconnectionEventArgs args)
        {
            Console.WriteLine($"Client disconnected: {args.Reason}");
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine($"Message {Encoding.UTF8.GetString(args.Data)}");
        }
    }
}

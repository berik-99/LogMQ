using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace LightTcp
{
    public class LightTcpClient(string ipAddress, int port) : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool disposed = false;

        public event EventHandler<byte[]> MessageReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public bool IsConnected => client?.Connected ?? false;
        public int Port { get; } = port;
        public string IPAddress { get; } = ipAddress;

        public void Connect()
        {
            client = new TcpClient();
            client.Connect(IPAddress, Port);
            stream = client.GetStream();
            OnConnected();
            Task.Run(ReceiveMessages);
        }

        public void Disconnect()
        {
            if (client?.Connected != true)
                throw new InvalidOperationException("Client is not connected.");
            client.Close();
            OnDisconnected();
        }

        public void Send(string message) => Send(Encoding.ASCII.GetBytes(message));

        public void Send(byte[] data)
        {
            CheckIfConnected();
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            stream?.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream?.Write(data, 0, data.Length);
        }

        public async Task SendAsync(string message) => await SendAsync(Encoding.ASCII.GetBytes(message));

        public async Task SendAsync(byte[] data)
        {
            CheckIfConnected();
            if (stream != null)
            {
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                await stream.WriteAsync(lengthPrefix);
                await stream.WriteAsync(data);
            }
        }

        public PingReply Ping()
        {
            using Ping pingSender = new();
            PingOptions options = new(4, true);
            byte[] buffer = Encoding.ASCII.GetBytes("PING");
            const int timeout = 120;
            try
            {
                PingReply reply = pingSender.Send(IPAddress, timeout, buffer, options);
                return reply;
            }
            catch (PingException ex)
            {
                throw new InvalidOperationException($"Ping failed: {ex.Message}", ex);
            }
        }

        private async Task ReceiveMessages()
        {
            if (stream == null)
            {
                throw new InvalidOperationException("Stream is not available.");
            }

            byte[] lengthBuffer = new byte[sizeof(int)];
            //byte[] buffer = new byte[1024];

            try
            {
                while (client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(lengthBuffer);
                    if (bytesRead == 0)
                        break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    int totalBytesRead = 0;
                    byte[] messageBuffer = new byte[messageLength];
                    while (totalBytesRead < messageLength)
                    {
                        bytesRead = await stream.ReadAsync(messageBuffer.AsMemory(totalBytesRead, messageLength - totalBytesRead));
                        if (bytesRead == 0)
                            break;
                        totalBytesRead += bytesRead;
                    }

                    OnMessageReceived(messageBuffer);
                }
            }
            finally
            {
                OnDisconnected();
            }
        }

        private void CheckIfConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Client is not connected.");
        }

        protected virtual void OnMessageReceived(byte[] data) => MessageReceived?.Invoke(this, data);

        protected virtual void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);

        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                stream?.Close();
                client?.Close();
            }
            disposed = true;
        }

        ~LightTcpClient() => Dispose(false);
    }
}

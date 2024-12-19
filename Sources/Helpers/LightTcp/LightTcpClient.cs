using System.Net.Sockets;
using System.Text;

namespace LightTcp
{
    public class LightTcpClient(string ipAddress, int port)
    {
        private TcpClient client;
        private NetworkStream stream;

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
            stream?.Write(data, 0, data.Length);
        }

        public async Task SendAsync(string message) => await SendAsync(Encoding.ASCII.GetBytes(message));

        public async Task SendAsync(byte[] data)
        {
            CheckIfConnected();
            if (stream != null)
            {
                await stream.WriteAsync(data);
            }
        }

        public byte[] Receive()
        {
            if (stream == null)
                throw new InvalidOperationException("Stream is not available.");

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] receivedData = new byte[bytesRead];
            Array.Copy(buffer, receivedData, bytesRead);
            return receivedData;
        }

        public async Task<byte[]> ReceiveAsync()
        {
            if (stream == null)
                throw new InvalidOperationException("Stream is not available.");

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer);
            byte[] receivedData = new byte[bytesRead];
            Array.Copy(buffer, receivedData, bytesRead);
            return receivedData;
        }

        private async Task ReceiveMessages()
        {
            if (stream == null)
            {
                throw new InvalidOperationException("Stream is not available.");
            }

            byte[] buffer = new byte[1024];

            try
            {
                while (client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead > 0)
                    {
                        byte[] receivedData = new byte[bytesRead];
                        Array.Copy(buffer, receivedData, bytesRead);
                        OnMessageReceived(receivedData);
                    }
                }
            }
            finally
            {
                OnDisconnected();
            }
        }

        private void CheckIfConnected()
        {
            if (client?.Connected != true)
                throw new InvalidOperationException("Client is not connected.");
        }

        protected virtual void OnMessageReceived(byte[] data) => MessageReceived?.Invoke(this, data);

        protected virtual void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);

        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);
    }
}
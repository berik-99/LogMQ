using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LightTcp
{
    public class LightTcpServer(string ipAddress, int port) : IDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Parse(ipAddress), port);
        private bool isRunning;
        private readonly ConcurrentDictionary<Guid, TcpClient> clients = new();
        private bool disposed = false;

        public event EventHandler<byte[]> MessageReceived;
        public event EventHandler<Guid> ClientConnected;
        public event EventHandler<Guid> ClientDisconnected;

        public bool IsRunning => isRunning;
        public List<Guid> Clients => [.. clients.Keys];
        public int ClientCount => clients.Keys.Count;
        public int Port => port;
        public string IpAddress => ipAddress;

        public void Start()
        {
            listener.Start();
            isRunning = true;
            Task.Run(AcceptClients);
        }

        public void Stop()
        {
            isRunning = false;
            listener.Stop();
            foreach (Guid clientId in clients.Keys)
                DisconnectClient(clientId);
        }

        public void Send(Guid clientId, string message) => Send(clientId, Encoding.ASCII.GetBytes(message));

        public void Send(Guid clientId, byte[] data)
        {
            if (clients.TryGetValue(clientId, out TcpClient client))
            {
                if (client?.Connected == true)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    throw new InvalidOperationException($"Client {clientId} is not connected.");
                }
            }
            else
            {
                throw new ArgumentException($"Client {clientId} not found.");
            }
        }

        public void Broadcast(string message) => Broadcast(Encoding.ASCII.GetBytes(message));

        public void Broadcast(byte[] data)
        {
            foreach (Guid clientId in clients.Keys)
                Send(clientId, data);
        }

        public async Task SendAsync(Guid clientId, string message) => await SendAsync(clientId, Encoding.ASCII.GetBytes(message));

        public async Task SendAsync(Guid clientId, byte[] data)
        {
            if (clients.TryGetValue(clientId, out TcpClient client))
            {
                if (client?.Connected == true)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                    await stream.WriteAsync(lengthPrefix);
                    await stream.WriteAsync(data);
                }
                else
                {
                    throw new InvalidOperationException($"Client {clientId} is not connected.");
                }
            }
            else
            {
                throw new ArgumentException($"Client {clientId} not found.");
            }
        }

        public async Task BroadcastAsync(string message) => await BroadcastAsync(Encoding.ASCII.GetBytes(message));

        public async Task BroadcastAsync(byte[] data)
        {
            foreach (Guid clientId in clients.Keys)
                await SendAsync(clientId, data);
        }

        public void DisconnectClient(Guid clientId)
        {
            if (clients.TryRemove(clientId, out TcpClient client))
            {
                client.Close();
                OnClientDisconnected(clientId);
            }
            else
            {
                throw new ArgumentException($"Client {clientId} not found.");
            }
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Common.EnableKeepAlive(client);
                    Guid clientId = Guid.NewGuid();
                    clients.TryAdd(clientId, client);
                    OnClientConnected(clientId);

                    Thread clientThread = new(() => HandleClient(clientId, client));
                    clientThread.Start();
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    break;
                }
            }
        }

        private void HandleClient(Guid clientId, TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] lengthBuffer = new byte[sizeof(int)];
            //byte[] buffer = new byte[1024];
            int bytesRead;
            try
            {
                while (client.Connected)
                {
                    bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0)
                        break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    int totalBytesRead = 0;
                    byte[] messageBuffer = new byte[messageLength];
                    while (totalBytesRead < messageLength)
                    {
                        bytesRead = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                        if (bytesRead == 0)
                            break;
                        totalBytesRead += bytesRead;
                    }

                    OnMessageReceived(messageBuffer);
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException)
                {
                    var error = (ex.InnerException as SocketException).SocketErrorCode;
                    if (error != SocketError.ConnectionAborted && error != SocketError.ConnectionReset)
                        return;
                }
            }
            finally
            {
                client.Close();
                clients.TryRemove(clientId, out _);
                OnClientDisconnected(clientId);
            }
        }

        protected virtual void OnMessageReceived(byte[] data) => MessageReceived?.Invoke(this, data);

        protected virtual void OnClientConnected(Guid clientId) => ClientConnected?.Invoke(this, clientId);

        protected virtual void OnClientDisconnected(Guid clientId) => ClientDisconnected?.Invoke(this, clientId);

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
                foreach (var client in clients.Values)
                {
                    client.Close();
                }
                listener.Stop();
            }
            disposed = true;
        }

        ~LightTcpServer() => Dispose(false);
    }
}
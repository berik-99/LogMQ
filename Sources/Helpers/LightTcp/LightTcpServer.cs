using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LightTcp
{
    public class LightTcpServer(string ipAddress, int port)
    {
        private readonly TcpListener listener = new(IPAddress.Parse(ipAddress), port);
        private bool isRunning;
        private readonly ConcurrentDictionary<Guid, TcpClient> clients = new();

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
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);
                    OnMessageReceived(receivedData);
                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionAborted) { }
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
    }
}
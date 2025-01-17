using System.Net.Sockets;

namespace LightTcp;

public static class Common
{
    public static void EnableKeepAlive(TcpClient client)
    {
        Socket socket = client.Client;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        if (OperatingSystem.IsWindows())
        {
            // Impostazione dei valori di keepalive per Windows (in millisecondi)
            byte[] keepAliveVals = new byte[12];
            BitConverter.GetBytes((uint)1).CopyTo(keepAliveVals, 0);  // Abilita keepalive
            BitConverter.GetBytes((uint)30000).CopyTo(keepAliveVals, 4);  // Mantieni viva ogni 30 secondi
            BitConverter.GetBytes((uint)10000).CopyTo(keepAliveVals, 8);  // Ritardo di 10 secondi tra i tentativi
            socket.IOControl(IOControlCode.KeepAliveValues, keepAliveVals, null);
        }
        else
        {
            // Impostazione dei valori di keepalive per Linux e macOS (in secondi)
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
        }
    }
}

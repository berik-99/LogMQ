using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogMQ.Extensions;
using System.Diagnostics;
using WatsonTcp;

namespace Serilog.Sinks.LogMQ;

public class LogMQBrokerSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider _formatProvider;
    private readonly string _applicationName;
    private readonly WatsonTcpClient tcpClient;

    public LogMQBrokerSink(IFormatProvider formatProvider, string host, int port, string applicationName = null)
    {
        //TODO: handle exceptions
        _formatProvider = formatProvider;
        _applicationName = string.IsNullOrWhiteSpace(applicationName) ? Process.GetCurrentProcess().ProcessName : applicationName;
        tcpClient = new WatsonTcpClient(host, port);
        tcpClient.Events.MessageReceived += (s, e) => { };
        tcpClient.Connect();
    }

    public void Emit(LogEvent logEvent)
    {
        //TODO: handle exceptions
        var logMsg = logEvent.ToLogMessage(_formatProvider, _applicationName);
        var bin = logMsg.Serialize();
        tcpClient.SendAsync(bin).Wait();
    }

    public void Dispose()
    {
        if (tcpClient?.Connected == true)
            tcpClient.Disconnect();
        tcpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
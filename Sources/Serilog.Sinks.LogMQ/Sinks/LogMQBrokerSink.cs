using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.LogMQ.Extensions;
using System.Text;
using WatsonTcp;

namespace Serilog.Sinks.LogMQ;

public class LogMQBrokerSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider _formatProvider;
    private readonly string _applicationName;
    private readonly string _category;
    private readonly WatsonTcpClient _tcpClient;
    private readonly ILogEventSink _fallbackLogger;

    internal LogMQBrokerSink(IFormatProvider formatProvider, string host, int port, string applicationName, string category, ILogEventSink fallbackLogger)
    {
        try
        {
            _tcpClient = new WatsonTcpClient(host, port);
            _tcpClient.Events.MessageReceived += (s, e) => { };
            _tcpClient.Connect();
            Task.Run(Ping).Wait();
        }
        catch (AggregateException aex)
        {
            (_fallbackLogger as Logger)?.Error(aex.GetBaseException(), "An aggregate exception occurred during LogMQBrokerSink initialization");
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "An error occurred during LogMQBrokerSink initialization");
        }
    }

    private async Task Ping()
    {
        try
        {
            byte[] message = Encoding.ASCII.GetBytes("PING");
            var res = await _tcpClient.SendAndWaitAsync(2000, message);
            string pong = Encoding.UTF8.GetString(res.Data);
            if (pong != "PONG")
                throw new InvalidOperationException("Unexpected response from LogMQ Broker: Expected 'PONG'");
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "Error occurred during the Ping operation");
            throw;
        }
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            var logMsg = logEvent.ToLogMessage(_formatProvider, _applicationName, _category);
            var bin = logMsg.Serialize();
            _tcpClient.SendAsync(bin).Wait();
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "Error occurred while writing log to LogMQ Broker");
            _fallbackLogger.Emit(logEvent);
        }
    }

    public void Dispose()
    {
        try
        {
            if (_tcpClient?.Connected == true)
                _tcpClient.Disconnect();
            _tcpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            (_fallbackLogger as Logger)?.Error(ex, "Error occurred during LogMQBrokerSink disposal");
        }
    }
}

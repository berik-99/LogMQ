using System.Text.Json;
using System.Text.Json.Serialization;
using WatsonTcp;

namespace LogMQ;

public static class MQLogger
{

    private static LogMQConfiguration config;
    private readonly static JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new LogProviderConverter() },
        PropertyNameCaseInsensitive = true
    };

    public static void LoadConfiguration() => LoadConfiguration("logmqsettings.json");

    public static void LoadConfiguration(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<LogMQConfiguration>(json, jsonOptions);
        LoadConfiguration(config);
    }

    public static void LoadConfiguration(LogMQConfiguration config)
    {
        //TODO: check for null or invalid configuration
        MQLogger.config = config;
    }

    public static void Log(string message)
    {
        foreach (var provider in config.Providers)
            provider.Log(message);
    }

}

public class LogMQConfiguration
{
    public List<ILogProvider> Providers { get; set; }
}

public interface ILogProvider : IDisposable
{
    abstract string MinLevel { get; set; }

    abstract void Log(string message);
}

public class ConsoleProvider : ILogProvider
{
    public string MinLevel { get; set; }
    public string Format { get; set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Log(string message)
    {
        Console.WriteLine($"PROVIDER: {typeof(ConsoleProvider)}; MESSAGE: {message}");
    }
}

public class FileProvider : ILogProvider
{
    public string MinLevel { get; set; }
    public string FilePath { get; set; }
    public string RollingInterval { get; set; }
    public string FileNameFormat { get; set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Log(string message)
    {
        Console.WriteLine($"PROVIDER: {typeof(FileProvider)}; MESSAGE: {message}");
    }
}

public class BrokerProvider : ILogProvider
{
    public string MinLevel { get; set; }
    public string BrokerAddress { get; set; }
    public int BrokerPort { get; set; }
    public string QueueName { get; set; }

    private WatsonTcpClient client;

    private void InitTcpClient()
    {
        client = new(BrokerAddress, BrokerPort);
        client.Events.MessageReceived += Events_MessageReceived;
    }

    public void Log(string message)
    {
        try
        {
            if (client is null)
                InitTcpClient();
            if (!client.Connected)
                client.Connect();
            client.SendAsync(message).Wait();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void Events_MessageReceived(object sender, MessageReceivedEventArgs e) { }

    public void Dispose()
    {
        if (client.Connected)
            client.Disconnect();
        client.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class LogProviderConverter : JsonConverter<ILogProvider>
{
    public override ILogProvider Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        string type = root.GetProperty("Type").GetString();
        return type switch
        {
            "Console" => JsonSerializer.Deserialize<ConsoleProvider>(root.GetRawText(), options),
            "File" => JsonSerializer.Deserialize<FileProvider>(root.GetRawText(), options),
            "Broker" => JsonSerializer.Deserialize<BrokerProvider>(root.GetRawText(), options),
            _ => throw new NotSupportedException($"Provider type '{type}' is not supported")
        };
    }

    public override void Write(Utf8JsonWriter writer, ILogProvider value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Serialization is not supported");
    }
}



/// <summary>
/// Defines logging severity levels.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Logs that contain the most detailed messages. These messages may contain sensitive application data.
    /// These messages are disabled by default and should never be enabled in a production environment.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Logs that are used for interactive investigation during development.  These logs should primarily contain
    /// information useful for debugging and have no long-term value.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Logs that track the general flow of the application. These logs should have long-term value.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the
    /// application execution to stop.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a
    /// failure in the current activity, not an application-wide failure.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires
    /// immediate attention.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Not used for writing log messages. Specifies that a logging category should not write any messages.
    /// </summary>
    None = 6,
}

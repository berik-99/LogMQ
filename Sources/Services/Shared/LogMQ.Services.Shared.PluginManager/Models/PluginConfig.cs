using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

public class PluginConfig
{
    private int enabledVersionIndex = -1;

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string EntryPoint { get; set; }
    public HashSet<Version> Versions { get; set; }

    public Version EnabledVersion
    {
        get => Versions.ElementAtOrDefault(enabledVersionIndex);
        set => enabledVersionIndex = Versions.Contains(value) ? Versions.ToList().IndexOf(value) : -1;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PluginType Type { get; set; }
}
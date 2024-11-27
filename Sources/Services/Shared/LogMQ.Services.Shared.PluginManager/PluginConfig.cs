using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager;

public class PluginConfig
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public PluginType Type { get; set; }
    public string EntryPoint { get; set; }
    public List<PluginVersionInfo> Versions { get; set; }

    [JsonIgnore]
    public PluginVersionInfo ActiveVersion => Versions?.First(v => v.Status == PluginStatus.Enabled);
}
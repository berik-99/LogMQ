using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

public class PluginManifest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Version Version { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public PluginType Type { get; set; }
    public string EntryPoint { get; set; }
}
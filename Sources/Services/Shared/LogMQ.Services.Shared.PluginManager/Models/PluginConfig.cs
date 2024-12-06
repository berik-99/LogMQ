using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

public class PluginConfig
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Author { get; set; }
	public string Description { get; set; }
	public string EntryPoint { get; set; }
	public HashSet<Version> Versions { get; set; }
	public int EnabledVersionIndex { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public PluginType Type { get; set; }

	[JsonIgnore]
	public Version ActiveVersion => EnabledVersionIndex == -1 ? null : Versions.ElementAt(EnabledVersionIndex);
}
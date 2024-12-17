using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

public class PluginVersion : IEquatable<PluginVersion>
{
    public Version Version { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VersionStatus Status { get; set; }

    public bool Equals(PluginVersion other) => Version == other.Version;
    public override bool Equals(object obj) => Equals(obj as PluginVersion);
    public override int GetHashCode() => Version.GetHashCode();
}

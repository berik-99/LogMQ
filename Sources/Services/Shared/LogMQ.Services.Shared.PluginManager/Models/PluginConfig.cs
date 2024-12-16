using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

public class PluginConfig : PluginManifest
{
    private int enabledVersionIndex = -1;

    public DateTime LastChangeDate { get; set; }

    public HashSet<ConfigVersion> Versions { get; set; }

    public new Version CurrentVersion
    {
        get => Versions.ElementAtOrDefault(enabledVersionIndex)?.Version;
        set => enabledVersionIndex = Versions.Exists(x => x.Version == value) ? Versions.ToList().FindIndex(x => x.Version == value) : -1;
    }
}

public class ConfigVersion : IEquatable<ConfigVersion>
{
    public Version Version { get; set; }
    public DateTime InsallDate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VersionStatus Status { get; set; }

    public bool Equals(ConfigVersion other) => Version == other.Version;

    public override bool Equals(object obj) => Equals(obj as ConfigVersion);

    public override int GetHashCode()
    {
        return Version.GetHashCode();
    }
}

public static class HashSetExtensions
{
    public static bool Exists<T>(this HashSet<T> hashSet, Func<T, bool> predicate) => hashSet.Any(predicate);

    public static bool AddOrReplace<T>(this HashSet<T> hashSet, T newValue)
    {
        hashSet.Remove(newValue);
        return hashSet.Add(newValue);
    }


}
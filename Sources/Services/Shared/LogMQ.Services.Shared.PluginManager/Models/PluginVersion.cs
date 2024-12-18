using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

/// <summary>
/// Represents a specific version of a plugin, including its version number and status.
/// </summary>
public class PluginVersion : IEquatable<PluginVersion>
{
    /// <summary>
    /// Gets or sets the version number of the plugin.
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// Gets or sets the status of the plugin version.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VersionStatus Status { get; set; }

    /// <summary>
    /// Determines whether the specified <see cref="PluginVersion"/> is equal to the current <see cref="PluginVersion"/>.
    /// </summary>
    /// <param name="other">The <see cref="PluginVersion"/> to compare with the current <see cref="PluginVersion"/>.</param>
    /// <returns>true if the specified <see cref="PluginVersion"/> is equal to the current <see cref="PluginVersion"/>; otherwise, false.</returns>
    public bool Equals(PluginVersion other) => Version == other.Version;

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="PluginVersion"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="PluginVersion"/>.</param>
    /// <returns>true if the specified object is equal to the current <see cref="PluginVersion"/>; otherwise, false.</returns>
    public override bool Equals(object obj) => Equals(obj as PluginVersion);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current <see cref="PluginVersion"/>.</returns>
    public override int GetHashCode() => Version.GetHashCode();
}
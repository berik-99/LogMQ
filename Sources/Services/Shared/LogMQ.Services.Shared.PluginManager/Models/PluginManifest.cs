namespace LogMQ.Services.Shared.PluginManager.Models;

/// <summary>
/// Represents the manifest for a plugin, including its version.
/// Inherits from <see cref="PluginBase"/>.
/// </summary>
public class PluginManifest : PluginBase
{
    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public Version Version { get; set; }
}
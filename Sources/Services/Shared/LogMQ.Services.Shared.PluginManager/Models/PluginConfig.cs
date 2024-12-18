namespace LogMQ.Services.Shared.PluginManager.Models;

/// <summary>
/// Represents the configuration for a plugin, including its versions.
/// Inherits from <see cref="PluginBase"/>.
/// </summary>
public class PluginConfig : PluginBase
{
    /// <summary>
    /// Gets or sets the collection of versions for the plugin.
    /// </summary>
    public HashSet<PluginVersion> Versions { get; set; }
}
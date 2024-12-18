using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginManager.Models;

/// <summary>
/// Represents the base class for plugin-related entities.
/// Contains common properties and methods shared by all plugin entities.
/// </summary>
public abstract class PluginBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the author of the plugin.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the entry point of the plugin.
    /// </summary>
    public string EntryPoint { get; set; }

    /// <summary>
    /// Gets or sets the publication date of the plugin.
    /// </summary>
    public DateTime PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets the type of the plugin.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PluginType Type { get; set; }

    /// <summary>
    /// Gets the installed path of the plugin for a specific version.
    /// </summary>
    /// <param name="version">The version of the plugin.</param>
    /// <returns>The installed path of the plugin.</returns>
    public string GetInstalledPath(Version version) => Path.Combine(IPluginManager.PluginBinariesFolder, Id.ToString(), $"{version}.lmqex");
}
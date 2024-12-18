namespace LogMQ.Services.Shared.PluginManager;

/// <summary>
/// Provides default paths and filenames used for plugin management in the LogMQ application.
/// </summary>
public static class Defaults
{
    /// <summary>
    /// The folder where all the plugins are stored.
    /// </summary>
    public static readonly string PluginFolder = Path.Combine(Common.Defaults.DataFolder, "Plugins");

    /// <summary>
    /// The file path for the staged plugin configuration file.
    /// </summary>
    public static readonly string PluginStagedConfigFile = Path.Combine(PluginFolder, "pluginconfig.staged.json");

    /// <summary>
    /// The file path for the running plugin configuration file.
    /// </summary>
    public static readonly string PluginRunningConfigFile = Path.Combine(PluginFolder, "pluginconfig.running.json");

    /// <summary>
    /// The folder where the plugin binaries are stored.
    /// </summary>
    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");

    /// <summary>
    /// The filename used for the plugin manifest.
    /// </summary>
    public const string PluginManifestFile = "manifest.json";

    /// <summary>
    /// Gets the installed path of the plugin for a specific version.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <param name="version">The version of the plugin.</param>
    /// <returns>The installed path of the plugin.</returns>
    public static string GetInstalledPath(Guid pluginId, Version version) => Path.Combine(PluginBinariesFolder, pluginId.ToString(), $"{version}.lmqex");
}
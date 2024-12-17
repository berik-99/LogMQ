using LogMQ.Services.Shared.PluginManager.Models;

namespace LogMQ.Services.Shared.PluginManager;

public interface IPluginManager
{
    public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LogMQ", "Plugins");
    public static readonly string PluginStagedConfigFile = Path.Combine(PluginFolder, "pluginconfig.staged.json");
    public static readonly string PluginRunningConfigFile = Path.Combine(PluginFolder, "pluginconfig.running.json");
    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
    public const string PluginManifestFile = "manifest.json";

    /// <summary>
    /// Analyzes a plugin file and extracts its manifest information.
    /// </summary>
    /// <param name="pluginPath">Path to the plugin file.</param>
    /// <returns>The plugin manifest.</returns>
    /// <exception cref="FileNotFoundException">Thrown when plugin file or manifest is not found.</exception>
    Task<PluginManifest> AnalyzePluginFile(string pluginPath);

    /// <summary>
    /// Retrieves plugin information by ID or name.
    /// </summary>
    /// <param name="configType">The type of configuration to query.</param>
    /// <param name="pluginReference">Plugin ID (GUID) or name.</param>
    /// <returns>The plugin configuration if found, null otherwise.</returns>
    Task<PluginConfig> GetPluginInfo(PluginConfigType configType, string pluginReference);

    /// <summary>
    /// Installs a plugin from a specified path.
    /// </summary>
    /// <param name="pluginPath">The file path to the plugin package.</param>
    /// <param name="overwrite">If true, overwrites existing plugin files.</param>
    /// <param name="enable">If true, enables the plugin after installation.</param>
    /// <param name="manifest">The plugin manifest containing metadata.</param>
    /// <returns>The configured plugin information.</returns>
    Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, bool enable, PluginManifest manifest);

    /// <summary>
    /// Uninstalls specified versions of a plugin.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <param name="versions">List of versions to uninstall.</param>
    /// <returns>The updated plugin configuration.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when a specified version is not found.</exception>
    Task<PluginConfig> UninstallPluginAsync(Guid pluginId, List<Version> versions);

    /// <summary>
    /// Enables a specific version of a plugin.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <param name="version">The version to enable.</param>
    /// <returns>The updated plugin configuration.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified version is not found.</exception>
    Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version);

    /// <summary>
    /// Disables a plugin by setting its status to Installed.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin to disable.</param>
    /// <returns>The updated plugin configuration.</returns>
    Task<PluginConfig> DisablePluginAsync(Guid pluginId);

    /// <summary>
    /// Lists plugins based on configuration type and type filter.
    /// </summary>
    /// <param name="configType">The type of configuration to query (Staged or Running).</param>
    /// <param name="typeFilter">List of plugin types to filter by.</param>
    /// <returns>List of matching plugin configurations.</returns>
    Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, List<PluginType> typeFilter);

    /// <summary>
    /// Restores plugin configuration from running to staged state.
    /// </summary>
    /// <param name="hardCopy">If true, performs a direct copy of the running configuration.</param>
    /// <returns>List of restored plugin configurations.</returns>
    Task<List<PluginConfig>> RestorePluginConfigAsync(bool hardCopy);

    /// <summary>
    /// Gets the running version of a specific plugin.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <returns>The version number of the running plugin, or null if not found.</returns>
    Task<Version> GetRunningVersion(Guid pluginId);
}
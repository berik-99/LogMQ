namespace LogMQ.Services.Shared.PluginManager;

/// <summary>
/// Interface for managing plugins and broker operations in LogMQ.
/// </summary>
public interface IPluginManager
{
    // Plugin Management

    /// <summary>
    /// Installs a plugin from the specified path.
    /// </summary>
    /// <param name="pluginPath">Path to the plugin file (.lmqex).</param>
    /// <param name="forceRestart">Indicates whether the broker should be restarted after installation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InstallPluginAsync(string pluginPath, bool forceRestart = false);

    /// <summary>
    /// Enables a plugin by its GUID, optionally targeting a specific version.
    /// </summary>
    /// <param name="pluginId">The GUID of the plugin.</param>
    /// <param name="version">Optional specific version of the plugin.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnablePluginAsync(Guid pluginId, Version version = null);

    /// <summary>
    /// Disables a plugin by its GUID, optionally targeting a specific version.
    /// </summary>
    /// <param name="pluginId">The GUID of the plugin.</param>
    /// <param name="version">Optional specific version of the plugin.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisablePluginAsync(Guid pluginId, Version version = null);

    /// <summary>
    /// Uninstalls a plugin by its GUID, optionally targeting a specific version.
    /// </summary>
    /// <param name="pluginId">The GUID of the plugin.</param>
    /// <param name="version">Optional specific version of the plugin.</param>
    /// <param name="forceRestart">Indicates whether the broker should be restarted after uninstallation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UninstallPluginAsync(Guid pluginId, Version version = null, bool forceRestart = false);

    /// <summary>
    /// Lists all plugins with optional filters.
    /// </summary>
    /// <param name="enabledOnly">Filter to show only enabled plugins.</param>
    /// <param name="disabledOnly">Filter to show only disabled plugins.</param>
    /// <param name="type">Optional plugin type filter (e.g., Receiver, Storage).</param>
    /// <returns>A list of plugin metadata objects.</returns>
    Task<IList<PluginConfig>> ListPluginsAsync(bool enabledOnly = false, bool disabledOnly = false, PluginType? type = null);

    /// <summary>
    /// Resets the plugin configuration file based on the binaries folder.
    /// </summary>
    /// <param name="dryRun">If true, simulates the reset and returns a report without applying changes.</param>
    /// <returns>A report of the changes to be applied.</returns>
    Task<string> ResetPluginConfigAsync(bool dryRun = false);

    // Broker Management

    /// <summary>
    /// Restarts the LogMQ broker.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RestartBrokerAsync();

    /// <summary>
    /// Gets the current status of the LogMQ broker.
    /// </summary>
    /// <returns>The broker status as a string.</returns>
    Task<string> GetBrokerStatusAsync();

    // Logging

    /// <summary>
    /// Retrieves the LogMQ logs.
    /// </summary>
    /// <param name="tail">If true, continuously retrieves the latest logs.</param>
    /// <param name="verbose">If true, provides detailed log output.</param>
    /// <returns>A list of log entries.</returns>
    Task<IList<string>> GetLogsAsync(bool tail = false, bool verbose = false);

    // Configuration Management

    /// <summary>
    /// Backs up the current configuration to the specified path.
    /// </summary>
    /// <param name="backupPath">Path to save the backup file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BackupConfigAsync(string backupPath);

    /// <summary>
    /// Restores the configuration from a specified backup file.
    /// </summary>
    /// <param name="backupPath">Path to the backup file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RestoreConfigAsync(string backupPath);

    // Additional Utility Methods

    /// <summary>
    /// Retrieves the GUID of a plugin by its name.
    /// </summary>
    /// <param name="pluginName">The name of the plugin.</param>
    /// <returns>The GUID of the plugin.</returns>
    Task<Guid> GetPluginIdByNameAsync(string pluginName);
}

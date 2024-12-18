namespace LogMQ.Services.Shared.PluginManager.Models;

/// <summary>
/// Specifies the type of the plugin.
/// </summary>
public enum PluginType
{
    /// <summary>
    /// Plugin responsible for receiving data.
    /// </summary>
    Receiver,

    /// <summary>
    /// Plugin responsible for storing data.
    /// </summary>
    Storage
}

/// <summary>
/// Specifies the configuration type of the plugin.
/// </summary>
public enum PluginConfigType
{
    /// <summary>
    /// Currently running configuration.
    /// </summary>
    Running,

    /// <summary>
    /// Staged configuration that is prepared but not yet active.
    /// </summary>
    Staged
}

/// <summary>
/// Specifies the status of a plugin version.
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// The plugin version is installed and active.
    /// </summary>
    Enabled,

    /// <summary>
    /// The plugin version is installed but not active.
    /// </summary>
    Installed,

    /// <summary>
    /// The plugin version is scheduled for removal and will be removed upon broker restart.
    /// </summary>
    Removed
}
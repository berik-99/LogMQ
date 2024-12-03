namespace LogMQ.Services.Shared.PluginManager.Models;

public enum PluginType
{
    Receiver,
    Storage
}

public enum PluginStatus
{
    None,       // No specific state assigned
    Enabled,    // Installed and active
    Disabled,   // Installed but not active
    Staged,     // Scheduled for installation
    Removed     // Scheduled for uninstallation
}

public enum PluginConfigType
{
    Running,
    Staged
}
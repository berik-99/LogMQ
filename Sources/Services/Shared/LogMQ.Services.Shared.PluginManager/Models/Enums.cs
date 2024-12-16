namespace LogMQ.Services.Shared.PluginManager.Models;

public enum PluginType
{
    Receiver,
    Storage
}

public enum PluginConfigType
{
    Running,
    Staged
}

public enum VersionStatus
{
    Added,
    Removed
}
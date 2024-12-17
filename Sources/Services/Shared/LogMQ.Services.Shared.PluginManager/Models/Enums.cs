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
    Enabled,        //Installed and active (check if is running)
    //Disabled,       //Scheduled for deactivation
    //Running,        //Installed and active
    Installed,      //Installed but not active
    Removed         //Scheduled for removal (is shown only for running versions till broker restarts)
}
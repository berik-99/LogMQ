namespace LogMQ.Services.Shared.PluginManager;

public enum PluginType
{
    Receiver,
    Storage
}

public enum PluginStatus
{
    None,       // Stato nullo
    Enabled,    // Installata e attiva
    Disabled,   // Installata ma non attiva
    Staged,   // Pianificata per l'installazione
    Removed    // Pianificata per la disinstallazione
}

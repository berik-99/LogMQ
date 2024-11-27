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
    Enlisted,   // Pianificata per l'installazione
    Delisted    // Pianificata per la disinstallazione
}

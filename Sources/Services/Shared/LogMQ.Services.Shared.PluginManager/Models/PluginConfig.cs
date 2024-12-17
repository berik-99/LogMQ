namespace LogMQ.Services.Shared.PluginManager.Models;

public class PluginConfig : PluginBase
{
    public HashSet<PluginVersion> Versions { get; set; }
}
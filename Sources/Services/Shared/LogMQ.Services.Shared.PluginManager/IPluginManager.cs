using LogMQ.Services.Shared.PluginManager.Models;

namespace LogMQ.Services.Shared.PluginManager;

public interface IPluginManager
{
    public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LogMQ", "Plugins");
    public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");
    public static readonly string PluginConfigBackupFile = $"{PluginConfigFile}.bak";
    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
    public const string PluginManifestFile = "manifest.json";
    Task<PluginManifest> AnalyzePluginFile(string pluginPath);
    Task<PluginConfig> DisablePluginAsync(Guid pluginId);
    Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version = null);
    //Task<Guid> GetPluginIdByNameAsync(PluginConfigType configType, string pluginName);
    Task<PluginConfig> GetPluginInfo(PluginConfigType configType, string pluginId);
    Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, bool enble, PluginManifest manifest = null);
    Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, bool showOnlyActive = true, PluginType? type = null);
    Task<List<PluginConfig>> RestorePluginConfigAsync();
    Task<PluginConfig> UninstallPluginAsync(Guid pluginId, Version version);
}
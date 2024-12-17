using LogMQ.Services.Shared.PluginManager.Models;

namespace LogMQ.Services.Shared.PluginManager;

public interface IPluginManager
{
    public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LogMQ", "Plugins");
    public static readonly string PluginStagedConfigFile = Path.Combine(PluginFolder, "pluginconfig.staged.json");
    public static readonly string PluginRunningConfigFile = Path.Combine(PluginFolder, "pluginconfig.running.json");
    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
    public const string PluginManifestFile = "manifest.json";
    Task<PluginManifest> AnalyzePluginFile(string pluginPath);
    Task<PluginConfig> DisablePluginAsync(Guid pluginId);
    Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version);
    Task<PluginConfig> GetPluginInfo(PluginConfigType configType, string pluginId);
    Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, bool enable, PluginManifest manifest);
    Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, List<PluginType> typeFilter);
    Task<List<PluginConfig>> RestorePluginConfigAsync(bool hardCopy);
    Task<PluginConfig> UninstallPluginAsync(Guid pluginId, List<Version> versions);
    Task<Version> GetRunningVersion(Guid pluginId);
}
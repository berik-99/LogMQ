using LogMQ.Services.Shared.PluginManager.Models;

namespace LogMQ.Services.Shared.PluginManager;

public interface IPluginManager
{
	public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");
	public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");
	public static readonly string PluginConfigBackupFile = $"{PluginConfigFile}.bak";
	public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
	public const string PluginManifestFile = "manifest.json";

	Task<PluginConfig> DisablePluginAsync(Guid pluginId, Version version = null);
	Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version = null);
	Task<PluginConfig> GetPluginConfigAsync(PluginConfigType configType, Guid pluginId);
	Task<Guid> GetPluginIdByNameAsync(PluginConfigType configType, string pluginName);
	Task<PluginConfig> GetPluginInfoAsync(PluginConfigType configType, string pluginPath);
	Task<PluginConfig> InstallPluginAsync(string pluginPath);
	Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, List<PluginStatus> statusFilter = null, PluginType? type = null);
	Task<List<PluginConfig>> RestorePluginConfigAsync();
	Task<PluginConfig> UninstallPluginAsync(Guid pluginId, Version version = null);
}

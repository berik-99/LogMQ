using LogMQ.Services.Shared.PluginManager.Models;
using System.IO.Compression;
using System.Text.Json;
using static LogMQ.Services.Shared.PluginManager.IPluginManager;

namespace LogMQ.Services.Shared.PluginManager;

public class PluginManager : IPluginManager
{
	public PluginManager()
	{
		if (!Path.Exists(PluginFolder))
			Directory.CreateDirectory(PluginFolder);
		if (!Path.Exists(PluginBinariesFolder))
			Directory.CreateDirectory(PluginBinariesFolder);
		if (!File.Exists(PluginConfigFile))
			File.AppendAllText(PluginConfigFile, JsonSerializer.Serialize(new List<PluginConfig>()));
		if (!File.Exists(PluginConfigBackupFile))
			File.Copy(PluginConfigFile, PluginConfigBackupFile);
	}

	public async Task<PluginConfig> DisablePluginAsync(Guid pluginId, Version version = null)
	{
		//await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		//List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
		//PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
		//if (plugin?.ActiveVersion != null && (version == null || plugin.ActiveVersion.Version == version))
		//{
		//	plugin.ActiveVersion.Status = PluginVersionStatus.Disabled;
		//	await File.WriteAllTextAsync(PluginConfigFile, JsonSerializer.Serialize(plugins));
		//}
		//await SavePluginsConfig(fileStream, plugins);
		//return plugin;
		throw new NotImplementedException();
	}

	public async Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version = null)
	{
		//await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		//List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
		//PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
		//if (plugin != null)
		//{
		//	version ??= plugin.Versions.Max(v => v.Version);
		//	plugin.Versions.ForEach(v => v.Status = v.Version == version ? PluginVersionStatus.Enabled : PluginVersionStatus.Disabled);
		//	await SavePluginsConfig(fileStream, plugins);
		//}
		//return plugin;

		throw new NotImplementedException();
	}

	public async Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, PluginManifest manifest = null)
	{
		manifest ??= await AnalyzePluginFile(pluginPath);
		var destFolder = Path.Combine(PluginBinariesFolder, manifest.Id.ToString());
		Directory.CreateDirectory(destFolder);
		var fileName = $"{manifest.Version}.lmqex";
		File.Copy(pluginPath, Path.Combine(destFolder, fileName), overwrite);
		List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
		var existingConfig = plugins.Find(p => p.Id == manifest.Id);
		if (existingConfig == null)
		{
			existingConfig = new PluginConfig
			{
				Id = manifest.Id,
				Name = manifest.Name,
				Author = manifest.Author,
				Description = manifest.Description,
				Type = manifest.Type,
				EntryPoint = manifest.EntryPoint,
				Versions = [manifest.Version]
			};
			plugins.Add(existingConfig);
		}
		else
		{
			existingConfig.Versions.Add(manifest.Version);
		}
		await SavePluginsConfig(plugins);
		return existingConfig;
	}

	public async Task<PluginConfig> UninstallPluginAsync(Guid pluginId, Version version = null)
	{
		//await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		//List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
		//PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
		//if (plugin?.ActiveVersion != null && (version == null || plugin.ActiveVersion.Version == version))
		//{
		//	plugin.ActiveVersion.Status = PluginVersionStatus.Removed;
		//	await File.WriteAllTextAsync(PluginConfigFile, JsonSerializer.Serialize(plugins));
		//}
		//await SavePluginsConfig(fileStream, plugins);
		//return plugin;

		throw new NotImplementedException();
	}

	public async Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, bool showAllStatus = false, PluginType? type = null)
	{
		List<PluginConfig> plugins = await LoadPluginsConfigAsync(configType);

		var filteredPlugins = plugins.Where(plugin =>
		{
			bool matchesStatus = statusFilter == null || statusFilter.Count == 0 ||
			plugin.Versions.Exists(version => statusFilter.Contains(version.Status));

			bool matchesType = type == null || plugin.Type == type;

			return matchesStatus && matchesType;
		}).ToList();

		return filteredPlugins;
	}

	public async Task<List<PluginConfig>> RestorePluginConfigAsync()
	{
		//File.Copy(PluginConfigFile, PluginConfigBackupFile);
		//await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		//List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
		//return plugins;

		throw new NotImplementedException();
	}

	public async Task<Guid> GetPluginIdByNameAsync(PluginConfigType configType, string pluginName)
	{
		List<PluginConfig> plugins = await LoadPluginsConfigAsync(configType);
		PluginConfig plugin = plugins.Find(p => p.Name == pluginName)
			?? throw new KeyNotFoundException($"Plugin {pluginName} not found");
		return plugin.Id;
	}

	public async Task<PluginManifest> AnalyzePluginFile(string pluginPath)
	{
		if (!Path.Exists(pluginPath))
			throw new FileNotFoundException($"Plugin {pluginPath} not found");
		await using var file = File.OpenRead(pluginPath);
		using var zip = new ZipArchive(file, ZipArchiveMode.Read);
		var manifestZipEntry = zip.Entries.FirstOrDefault(e => e.Name == PluginManifestFile)
			 ?? throw new FileNotFoundException("Plugin manifest not found");
		await using Stream manifestStream = manifestZipEntry.Open();
		using var reader = new StreamReader(manifestStream);
		string manifestContent = await reader.ReadToEndAsync();
		var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent);
		_ = zip.Entries.FirstOrDefault(x => x.Name == manifest.EntryPoint)
			?? throw new FileNotFoundException("Plugin entry point not found");
		//TODO: Verify entrypoint
		return manifest;
	}

	private static async Task<List<PluginConfig>> LoadPluginsConfigAsync(PluginConfigType configType)
	{
		var configFile = configType == PluginConfigType.Running ? PluginConfigBackupFile : PluginConfigFile;
		string json = await File.ReadAllTextAsync(configFile);
		return JsonSerializer.Deserialize<List<PluginConfig>>(json);
	}

	private static async Task SavePluginsConfig(List<PluginConfig> plugins)
	{
		var json = JsonSerializer.Serialize(plugins);
		await File.WriteAllTextAsync(PluginConfigFile, json);
	}
}

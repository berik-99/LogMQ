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
        if (!File.Exists(PluginStagedConfigFile))
            File.AppendAllText(PluginStagedConfigFile, JsonSerializer.Serialize(new List<PluginConfig>()));
        if (!File.Exists(PluginRunningConfigFile))
            File.Copy(PluginStagedConfigFile, PluginRunningConfigFile);
    }

    public async Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, bool enable, PluginManifest manifest)
    {
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
                Versions = [],
            };
            plugins.Add(existingConfig);
        }
        existingConfig.Versions.AddOrReplace(new ConfigVersion { Version = manifest.Version, Status = enable ? VersionStatus.Enabled : VersionStatus.Installed });
        plugins = SortConfig(plugins);
        await SavePluginsConfig(plugins);
        return existingConfig;
    }

    public async Task<PluginConfig> UninstallPluginAsync(Guid pluginId, List<Version> versions)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        foreach (var version in versions)
        {
            if (!plugin.Versions.Exists(x => x.Version == version))
                throw new KeyNotFoundException($"Version {version} not found for plugin {plugin.Name}");
        }

        foreach (var version in versions)
        {
            var pluginVersion = plugin.Versions.FirstOrDefault(x => x.Version == version);
            if (pluginVersion.Status == VersionStatus.Installed)
            {
                var dir = Path.Combine(PluginBinariesFolder, pluginId.ToString());
                File.Delete(Path.Combine(dir, $"{version}.lmqex"));
                if (Directory.GetFiles(dir).Length == 0)
                    Directory.Delete(dir);
                plugin.Versions.Remove(pluginVersion);
                if (plugin.Versions.Count == 0)
                    plugins.Remove(plugin);
            }
            else
            {
                pluginVersion.Status = VersionStatus.Removed;
            }
        }

        plugins = SortConfig(plugins);
        await SavePluginsConfig(plugins);
        return plugin;
    }

    public async Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (!plugin.Versions.Exists(x => x.Version == version))
            throw new KeyNotFoundException($"Version {version} not found for plugin {plugin.Name}");

        var selectedVersion = plugin.Versions.First(x => x.Version == version);
        selectedVersion.Status = VersionStatus.Enabled;
        await SavePluginsConfig(plugins);
        return plugin;
    }

    public async Task<PluginConfig> DisablePluginAsync(Guid pluginId)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        var currentActiveplugin = plugin.Versions.FirstOrDefault(x => x.Status == VersionStatus.Enabled);
        if (currentActiveplugin != null)
            currentActiveplugin.Status = VersionStatus.Installed;
        await SavePluginsConfig(plugins);
        return plugin;
    }

    public async Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, List<PluginType> typeFilter)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(configType);
        return plugins.FindAll(x => typeFilter.Contains(x.Type));
    }

    public async Task<List<PluginConfig>> RestorePluginConfigAsync()
    {
        File.Copy(PluginRunningConfigFile, PluginStagedConfigFile, true);
        return await ListPluginsAsync(PluginConfigType.Staged, null);
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

    public async Task<PluginConfig> GetPluginInfo(PluginConfigType configType, string pluginReference)
    {
        var plugins = await LoadPluginsConfigAsync(configType);
        if (Guid.TryParse(pluginReference, out Guid guid))
            return plugins.Find(x => x.Id == guid);
        else if (plugins.Exists(x => x.Name == pluginReference))
            return plugins.Find(x => x.Name == pluginReference);
        return null;
    }

    private static async Task<List<PluginConfig>> LoadPluginsConfigAsync(PluginConfigType configType)
    {
        var configFile = configType == PluginConfigType.Running ? PluginRunningConfigFile : PluginStagedConfigFile;
        string json = await File.ReadAllTextAsync(configFile);
        return JsonSerializer.Deserialize<List<PluginConfig>>(json);
    }

    private static async Task SavePluginsConfig(List<PluginConfig> plugins)
    {
        var json = JsonSerializer.Serialize(plugins);
        await File.WriteAllTextAsync(PluginStagedConfigFile, json);
    }

    private static List<PluginConfig> SortConfig(List<PluginConfig> config)
    {
        config = [.. config.OrderBy(x => x.Name)];
        config.ForEach(x => x.Versions = [.. x.Versions.OrderByDescending(x => x.Version)]);
        return config;
    }

    public async Task<Version> GetRunningVersion(Guid pluginId)
    {
        var plugins = await LoadPluginsConfigAsync(PluginConfigType.Running);
        return plugins.FirstOrDefault(x => x.Id == pluginId)?.Versions.FirstOrDefault(x => x.Status == VersionStatus.Enabled)?.Version;
    }
}

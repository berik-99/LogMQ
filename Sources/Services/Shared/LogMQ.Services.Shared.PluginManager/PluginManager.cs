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

    public async Task<PluginConfig> DisablePluginAsync(Guid pluginId)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        plugin.EnabledVersion = null;
        await SavePluginsConfig(plugins);
        return plugin;
    }

    public async Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version = null)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        version ??= plugin.Versions.Max();
        if (!plugin.Versions.Contains(version))
            throw new KeyNotFoundException($"Version {version} not found for plugin {plugin.Name}");
        plugin.EnabledVersion = version;
        await SavePluginsConfig(plugins);
        return plugin;
    }

    public async Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, bool enable, PluginManifest manifest = null)
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
                Versions = [manifest.Version],
                EnabledVersion = null
            };
            plugins.Add(existingConfig);
        }
        else
        {
            existingConfig.Versions.Add(manifest.Version);
        }
        if (enable) existingConfig.EnabledVersion = manifest.Version;
        plugins = SortConfig(plugins);
        await SavePluginsConfig(plugins);
        return existingConfig;
    }

    public async Task<PluginConfig> UninstallPluginAsync(Guid pluginId, Version version)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (!plugin.Versions.Contains(version))
            throw new KeyNotFoundException($"Version {version} not found for plugin {plugin.Name}");
        if (plugin.EnabledVersion == version)
            plugin.EnabledVersion = null;

        //TODO: run this at broker startup
        //var dir = Path.Combine(PluginBinariesFolder, pluginId.ToString());
        //File.Delete(Path.Combine(dir, $"{version}.lmqex"));
        //if (Directory.GetFiles(dir).Length == 0)
        //    Directory.Delete(dir);
        plugins = SortConfig(plugins);
        await SavePluginsConfig(plugins);
        return plugin;
    }

    public async Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, bool showOnlyActive = true, PluginType? type = null)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(configType);
        if (showOnlyActive)
        {
            plugins = plugins.Where(x => x.EnabledVersion != null).ToList();
            plugins.ForEach(plugin => plugin.Versions = plugin.Versions.Where(version => version == plugin.EnabledVersion).ToHashSet());
        }
        if (type != null)
            plugins = plugins.Where(x => x.Type == type).ToList();
        return plugins;
    }

    public async Task<List<PluginConfig>> RestorePluginConfigAsync()
    {
        File.Copy(PluginConfigFile, PluginConfigBackupFile);
        return await ListPluginsAsync(PluginConfigType.Staged, false, null);
    }

    //public async Task<Guid> GetPluginIdByNameAsync(PluginConfigType configType, string pluginName)
    //{
    //    List<PluginConfig> plugins = await LoadPluginsConfigAsync(configType);
    //    PluginConfig plugin = plugins.Find(p => p.Name == pluginName)
    //        ?? throw new KeyNotFoundException($"Plugin {pluginName} not found");
    //    return plugin.Id;
    //}

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
        var configFile = configType == PluginConfigType.Running ? PluginConfigBackupFile : PluginConfigFile;
        string json = await File.ReadAllTextAsync(configFile);
        return JsonSerializer.Deserialize<List<PluginConfig>>(json);
    }

    private static async Task SavePluginsConfig(List<PluginConfig> plugins)
    {
        var json = JsonSerializer.Serialize(plugins);
        await File.WriteAllTextAsync(PluginConfigFile, json);
    }

    private static List<PluginConfig> SortConfig(List<PluginConfig> config)
    {
        config = [.. config.OrderBy(x => x.Name)];
        config.ForEach(x => x.Versions = [.. x.Versions.OrderDescending()]);
        return config;
    }
}

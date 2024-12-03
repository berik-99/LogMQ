using LogMQ.Services.Shared.PluginManager.Models;
using System.IO.Compression;
using System.Text.Json;

namespace LogMQ.Services.Shared.PluginManager;

public static class PluginManager
{
    public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");
    public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");
    public static readonly string PluginConfigBackupFile = $"{PluginConfigFile}.bak";
    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
    public const string PluginManifestFile = "manifest.json";

    public static async Task<PluginConfig> DisablePluginAsync(Guid pluginId, Version version = null)
    {
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (plugin?.ActiveVersion != null && (version == null || plugin.ActiveVersion.Version == version))
        {
            plugin.ActiveVersion.Status = PluginStatus.Disabled;
            await File.WriteAllTextAsync(PluginConfigFile, JsonSerializer.Serialize(plugins));
        }
        await SavePluginsConfig(fileStream, plugins);
        return plugin;
    }

    public static async Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version = null)
    {
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (plugin != null)
        {
            version ??= plugin.Versions.Max(v => v.Version);
            plugin.Versions.ForEach(v => v.Status = v.Version == version ? PluginStatus.Enabled : PluginStatus.Disabled);
            await SavePluginsConfig(fileStream, plugins);
        }
        return plugin;
    }

    public static async Task<PluginConfig> InstallPluginAsync(string pluginPath)
    {
        if (!Path.Exists(pluginPath))
            throw new FileNotFoundException($"Plugin {pluginPath} not found");
        await using var file = File.OpenRead(pluginPath);
        using var zip = new ZipArchive(file, ZipArchiveMode.Read);

        var manifest = await GetPluginManifestAsync(zip);

        var entryPoint = zip.Entries.FirstOrDefault(x => x.Name == manifest.EntryPoint)
            ?? throw new FileNotFoundException("Plugin entry point not found");


        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        var existingConfig = plugins.Find(p => p.Id == manifest.Id);

        if (existingConfig?.Versions.Exists(x => x.Version == manifest.Version) == true)
            throw new InvalidOperationException($"Plugin {manifest.Name} version {manifest.Version} already installed");

        //TODO: Verify entrypoint

        var destFolder = Path.Combine(PluginBinariesFolder, manifest.Id.ToString());
        Directory.CreateDirectory(destFolder);
        var fileName = $"{manifest.Name}_{manifest.Version}";
        File.Copy(pluginPath, Path.Combine(destFolder, fileName));

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
                Versions =
                [
                    new PluginVersionInfo
                    {
                        Version = manifest.Version,
                        Status = PluginStatus.Staged
                    }
                ]
            };
            plugins.Add(existingConfig);
        }
        else
        {
            existingConfig.Versions.ForEach(x => x.Status = PluginStatus.Disabled);
            existingConfig.Versions.Add(new PluginVersionInfo
            {
                Version = manifest.Version,
                Status = PluginStatus.Staged
            });
        }
        await SavePluginsConfig(fileStream, plugins);
        return existingConfig;
    }

    public static async Task<PluginConfig> UninstallPluginAsync(Guid pluginId, Version version = null)
    {
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (plugin?.ActiveVersion != null && (version == null || plugin.ActiveVersion.Version == version))
        {
            plugin.ActiveVersion.Status = PluginStatus.Removed;
            await File.WriteAllTextAsync(PluginConfigFile, JsonSerializer.Serialize(plugins));
        }
        await SavePluginsConfig(fileStream, plugins);
        return plugin;
    }

    public static async Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, List<PluginStatus> statusFilter = null, PluginType? type = null)
    {
        var configFile = configType == PluginConfigType.Running ? PluginConfigBackupFile : PluginConfigFile;
        await using FileStream fileStream = new(configFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);

        var filteredPlugins = plugins.Where(plugin =>
        {
            bool matchesStatus = plugin.Versions.Exists(version => statusFilter?.Contains(version.Status) == true);
            bool matchesType = type == null || plugin.Type == type;
            return matchesStatus && matchesType;
        }).ToList();

        return filteredPlugins;
    }

    public static async Task<List<PluginConfig>> RestorePluginConfigAsync()
    {
        File.Copy(PluginConfigFile, PluginConfigBackupFile);
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        return plugins;
    }

    public static async Task<PluginConfig> GetPluginConfigAsync(PluginConfigType configType, Guid pluginId)
    {
        var configFile = configType == PluginConfigType.Running ? PluginConfigBackupFile : PluginConfigFile;
        await using FileStream fileStream = new(configFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        return plugins.Find(p => p.Id == pluginId);
    }

    public static async Task<PluginConfig> GetPluginInfoAsync(PluginConfigType configType, string pluginPath)
    {
        if (!Path.Exists(pluginPath))
            throw new FileNotFoundException($"Plugin {pluginPath} not found");
        await using var file = File.OpenRead(pluginPath);
        using var zip = new ZipArchive(file, ZipArchiveMode.Read);

        var manifest = await GetPluginManifestAsync(zip);

        _ = zip.Entries.FirstOrDefault(x => x.Name == manifest.EntryPoint)
            ?? throw new FileNotFoundException("Plugin entry point not found");

        var pluginConfig = await GetPluginConfigAsync(configType, manifest.Id);
        if (pluginConfig == null)
        {
            pluginConfig = new PluginConfig
            {
                Id = manifest.Id,
                Name = manifest.Name,
                Author = manifest.Author,
                Description = manifest.Description,
                Type = manifest.Type,
                EntryPoint = manifest.EntryPoint,
                Versions =
                [
                    new PluginVersionInfo
                    {
                        Version = manifest.Version,
                        Status = PluginStatus.None
                    }
                 ]
            };
        }
        else if (!pluginConfig.Versions.Exists(x => x.Version == manifest.Version))
        {
            pluginConfig.Versions.Add(new PluginVersionInfo
            {
                Version = manifest.Version,
                Status = PluginStatus.None
            });
        }
        return pluginConfig;
    }

    public static async Task<Guid> GetPluginIdByNameAsync(PluginConfigType configType, string pluginName)
    {
        var configFile = configType == PluginConfigType.Running ? PluginConfigBackupFile : PluginConfigFile;
        await using FileStream fileStream = new(configFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        PluginConfig plugin = plugins.Find(p => p.Name == pluginName)
            ?? throw new KeyNotFoundException($"Plugin {pluginName} not found");
        return plugin.Id;
    }

    private static async Task<List<PluginConfig>> LoadPluginsConfigAsync(FileStream fileStream)
    {
        using StreamReader reader = new(fileStream);
        fileStream.SetLength(0);
        fileStream.Seek(0, SeekOrigin.Begin);
        string json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<List<PluginConfig>>(json);
    }

    private static async Task SavePluginsConfig(FileStream fileStream, List<PluginConfig> plugins)
    {
        var json = JsonSerializer.Serialize(plugins);
        await using StreamWriter writer = new(fileStream);
        fileStream.SetLength(0);
        fileStream.Seek(0, SeekOrigin.Begin);
        await writer.WriteAsync(json);
        await writer.FlushAsync();
    }

    private static async Task<PluginManifest> GetPluginManifestAsync(ZipArchive zip)
    {
        var manifestZipEntry = zip.Entries.FirstOrDefault(e => e.Name == PluginManifestFile)
             ?? throw new FileNotFoundException("Plugin manifest not found");

        await using Stream manifestStream = manifestZipEntry.Open();
        using var reader = new StreamReader(manifestStream);
        string manifestContent = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent);
        return manifest;
    }
}

using LogMQ.Services.Shared.PluginManager.Models;
using System.IO.Compression;
using System.Text.Json;
using static LogMQ.Services.Shared.PluginManager.IPluginManager;

namespace LogMQ.Services.Shared.PluginManager;

public class PluginManager : IPluginManager
{
    /// <summary>
    /// Initializes a new instance of the PluginManager class.
    /// Creates necessary directories and configuration files if they don't exist.
    /// </summary>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<PluginConfig> GetPluginInfo(PluginConfigType configType, string pluginReference)
    {
        var plugins = await LoadPluginsConfigAsync(configType);
        if (Guid.TryParse(pluginReference, out Guid guid))
            return plugins.Find(x => x.Id == guid);
        else if (plugins.Exists(x => x.Name == pluginReference))
            return plugins.Find(x => x.Name == pluginReference);
        return null;
    }

    /// <inheritdoc/>
    public async Task<PluginConfig> InstallPluginAsync(string pluginPath, bool overwrite, bool enable, PluginManifest manifest)
    {
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
        existingConfig.Versions.RemoveWhere(x => x.Version == manifest.Version);
        var installedVersion = new PluginVersion { Version = manifest.Version, Status = enable ? VersionStatus.Enabled : VersionStatus.Installed };
        existingConfig.Versions.Add(installedVersion);

        var destFileName = existingConfig.GetInstalledPath(installedVersion.Version);
        Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
        File.Copy(pluginPath, destFileName, overwrite);

        plugins = SortConfig(plugins);
        await SavePluginsConfig(plugins);
        return existingConfig;
    }

    /// <inheritdoc/>
    public async Task<PluginConfig> UninstallPluginAsync(Guid pluginId, List<Version> versions)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        foreach (var version in versions)
        {
            if (!plugin.Versions.Any(x => x.Version == version))
                throw new KeyNotFoundException($"Version {version} not found for plugin {plugin.Name}");
        }

        foreach (var version in versions)
        {
            var pluginVersion = plugin.Versions.FirstOrDefault(x => x.Version == version);
            if (pluginVersion.Status == VersionStatus.Installed)
            {
                var destFileName = plugin.GetInstalledPath(pluginVersion.Version);
                var dir = Path.GetDirectoryName(destFileName);

                File.Delete(destFileName);
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

    /// <inheritdoc/>
    public async Task<PluginConfig> EnablePluginAsync(Guid pluginId, Version version)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(PluginConfigType.Staged);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (!plugin.Versions.Any(x => x.Version == version))
            throw new KeyNotFoundException($"Version {version} not found for plugin {plugin.Name}");

        foreach (var item in plugin.Versions)
            item.Status = item.Version == version ? VersionStatus.Enabled : VersionStatus.Installed;

        await SavePluginsConfig(plugins);
        return plugin;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<List<PluginConfig>> ListPluginsAsync(PluginConfigType configType, List<PluginType> typeFilter)
    {
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(configType);
        return plugins.FindAll(x => typeFilter.Contains(x.Type));
    }

    /// <inheritdoc/>
    public async Task<List<PluginConfig>> RestorePluginConfigAsync(bool hardCopy)
    {
        var filter = new List<PluginType> { PluginType.Receiver, PluginType.Storage };
        if (hardCopy)
        {
            File.Copy(PluginRunningConfigFile, PluginStagedConfigFile, true);
            return await ListPluginsAsync(PluginConfigType.Staged, filter);
        }

        var plugins = await ListPluginsAsync(PluginConfigType.Staged, filter);
        foreach (var version in plugins.SelectMany(plugin => plugin.Versions))
            version.Status = VersionStatus.Installed;
        File.Copy(PluginRunningConfigFile, PluginStagedConfigFile, true);
        await SavePluginsConfig(plugins);
        return plugins;
    }

    /// <inheritdoc/>
    public async Task<Version> GetRunningVersion(Guid pluginId)
    {
        var plugins = await LoadPluginsConfigAsync(PluginConfigType.Running);
        return plugins.FirstOrDefault(x => x.Id == pluginId)?.Versions.FirstOrDefault(x => x.Status == VersionStatus.Enabled)?.Version;
    }

    /// <summary>
    /// Loads plugin configurations from the specified configuration file.
    /// </summary>
    /// <param name="configType">The type of configuration to load.</param>
    /// <returns>List of plugin configurations.</returns>
    private static async Task<List<PluginConfig>> LoadPluginsConfigAsync(PluginConfigType configType)
    {
        var configFile = configType == PluginConfigType.Running ? PluginRunningConfigFile : PluginStagedConfigFile;
        string json = await File.ReadAllTextAsync(configFile);
        return JsonSerializer.Deserialize<List<PluginConfig>>(json);
    }

    /// <summary>
    /// Saves plugin configurations to the staged configuration file.
    /// </summary>
    /// <param name="plugins">List of plugin configurations to save.</param>
    private static async Task SavePluginsConfig(List<PluginConfig> plugins)
    {
        var json = JsonSerializer.Serialize(plugins);
        await File.WriteAllTextAsync(PluginStagedConfigFile, json);
    }

    /// <summary>
    /// Sorts plugin configurations by name and their versions in descending order.
    /// </summary>
    /// <param name="config">List of plugin configurations to sort.</param>
    /// <returns>Sorted list of plugin configurations.</returns>
    private static List<PluginConfig> SortConfig(List<PluginConfig> config)
    {
        config = [.. config.OrderBy(x => x.Name)];
        config.ForEach(x => x.Versions = [.. x.Versions.OrderByDescending(x => x.Version)]);
        return config;
    }
}

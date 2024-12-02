using LogMQ.Services.Shared.PluginManager.Models;
using System.Text.Json;

namespace LogMQ.Services.Shared.PluginManager;

public static class PluginManager //: IPluginManager
{
    public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");

    public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");
    public static readonly string PluginConfigBackupFile = $"{PluginConfigFile}.bak";

    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");

    //Plugin management

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
        throw new NotImplementedException();
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

    public static async Task<List<PluginConfig>> ListPluginsAsync(bool enabledOnly = false, bool disabledOnly = false, PluginType? type = null)
    {
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        //TODO: filter plugins
        return plugins;
    }

    //Restore plugin config from .bak file generated at broker startup which contains the running plugin configuration
    public static async Task<List<PluginConfig>> RestorePluginConfigAsync()
    {
        File.Copy(PluginConfigFile, PluginConfigBackupFile);
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        return plugins;
    }

    public static async Task<PluginConfig> GetPluginConfigAsync(Guid pluginId)
    {
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        return plugins.Find(p => p.Id == pluginId);
    }

    public static async Task<PluginConfig> GetPluginInfoAsync(string pluginPath)
    {
        if (!Path.Exists(pluginPath))
            throw new FileNotFoundException($"Plugin {pluginPath} not found");
        //TODO: unzip plugin
        //TODO: read plugin metadata
        PluginManifest manifest = new();
        PluginStatus status = await GetPluginConfigAsync(manifest.Id) != null ? PluginStatus.Enabled : PluginStatus.None;
        return new PluginConfig
        {
            Id = Guid.NewGuid(),
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
                    Status = status
                }
            ]
        };
    }

    public static async Task<Guid> GetPluginIdByNameAsync(string pluginName)
    {
        await using FileStream fileStream = new(PluginConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        List<PluginConfig> plugins = await LoadPluginsConfigAsync(fileStream);
        PluginConfig plugin = plugins.Find(p => p.Name == pluginName)
            ?? throw new KeyNotFoundException($"Plugin {pluginName} not found");
        return plugin.Id;
    }

    //Broker management

    public static async Task<string> GetBrokerStatusAsync()
    {
        throw new NotImplementedException();
    }

    public static async Task RestartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public static async Task StartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public static async Task StopBrokerAsync()
    {
        throw new NotImplementedException();
    }
}

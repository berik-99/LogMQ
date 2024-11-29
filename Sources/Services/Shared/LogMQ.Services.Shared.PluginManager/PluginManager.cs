using LogMQ.Services.Shared.PluginManager.Models;
using System.Text.Json;

namespace LogMQ.Services.Shared.PluginManager;

public class PluginManager //: IPluginManager
{
    public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");

    public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");

    public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");


    private static async Task<List<PluginConfig>> LoadPluginsConfig(FileStream fileStream)
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
        List<PluginConfig> plugins = await LoadPluginsConfig(fileStream);
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
        List<PluginConfig> plugins = await LoadPluginsConfig(fileStream);
        PluginConfig plugin = plugins.Find(p => p.Id == pluginId);
        if (plugin != null)
        {
            version ??= plugin.Versions.Max(v => v.Version);
            plugin.Versions.ForEach(v => v.Status = v.Version == version ? PluginStatus.Enabled : PluginStatus.Disabled);
            await SavePluginsConfig(fileStream, plugins);
        }
        return plugin;
    }

    public Task<string> GetBrokerStatusAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Guid> GetPluginIdByNameAsync(string pluginName)
    {
        throw new NotImplementedException();
    }

    public Task InstallPluginAsync(string pluginPath)
    {
        throw new NotImplementedException();
    }

    public Task<IList<PluginConfig>> ListPluginsAsync(bool enabledOnly = false, bool disabledOnly = false, PluginType? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> ResetPluginConfigAsync(bool dryRun = false)
    {
        throw new NotImplementedException();
    }

    public Task RestartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public Task StartBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public Task StopBrokerAsync()
    {
        throw new NotImplementedException();
    }

    public Task UninstallPluginAsync(Guid pluginId, Version version = null)
    {
        throw new NotImplementedException();
    }
}

namespace LogMQ.Services.Shared.PluginInstaller;

public static class Defaults
{
	//TODO: dont use hard-coded names
	public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");
	public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");
	public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
}

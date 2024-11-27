using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogMQ.Services.Shared.PluginInstaller;

public class PluginService
{
	public List<PluginEntry> AvailablePlugins { get; set; }

	public void UpdatePluginList(bool hardRefresh)
	{
		if (hardRefresh)
		{
			//TODO: Implement hard refresh with analizing plugins binaries folder and updating the plugin config file
		}
		AvailablePlugins = JsonSerializer.Deserialize<List<PluginEntry>>(File.ReadAllText(Defaults.PluginConfigFile));
	}

	public void EnablePlugin(Guid id, Version version)
	{
		// Find the plugin with the specified ID or throw an exception if not found
		var plugin = AvailablePlugins.FirstOrDefault(p => p.Id == id)
					 ?? throw new InvalidOperationException($"Plugin with ID {id} not found.");

		// Check if the specified version exists in the plugin's version list
		int versionIndex = plugin.Versions.FindIndex(v => v == version);
		if (versionIndex == -1)
			throw new InvalidOperationException($"Version {version} not found for the plugin with ID {id}.");

		// Set the active version index and enable the plugin
		plugin.ActiveVersion = versionIndex;
		plugin.Enabled = true;
	}

	public void InstallPlugin(string path)
	{
		//TODO: Implement plugin installation:
		// * Get information about the plugin from the .lmqex metadata file
		// * Check if the plugin is already installed
		// * Check if the plugin is compatible with the current version of the application
		// * Extract the plugin binaries to the plugin binaries folder
		// * Update the plugin list
		// * Enable the plugin
	}
}

public class PluginEntry
{
	public Guid Id { get; set; }                       // Identificativo unico del plugin
	public string Name { get; set; }                   // Nome del plugin
	public PluginType Type { get; set; }               // Tipo di plugin (Receiver/Storage)
	public List<PluginVersion> Versions { get; set; }  // Lista delle versioni del plugin

	[JsonIgnore]
	public PluginVersion ActiveVersion => Versions?.FirstOrDefault(v => v.Status == PluginStatus.Enabled);
}

public class PluginVersion
{
	public Version Version { get; set; }              // Numero di versione (es: 1.0.0)
	public PluginStatus Status { get; set; }          // Stato della versione
}

public enum PluginType
{
	Receiver,
	Storage
}

public enum PluginStatus
{
	Enabled,    // Installata e attiva
	Disabled,   // Installata ma non attiva
	Enlisted,   // Pianificata per l'installazione
	Delisted    // Pianificata per la disinstallazione
}

# Plugin Management in LogMQ

## Plugin Format

Plugins use the `.lmqex` extension. A `.lmqex` file is a ZIP archive containing:
- A `manifest.json` file with all the plugin metadata.
- A DLL file that implements the abstract class `LogMQReceiverBase`.

## Manifest Structure

The `manifest.json` file must follow this structure:
```json
{
  "Id": "string",          // A unique identifier for the plugin (GUID)
  "Name": "string",        // The name of the plugin
  "Version": "string",     // The plugin version (e.g., "1.0.0")
  "Author": "string",      // The plugin author
  "Description": "string", // A short description of the plugin
  "Type": "string",        // The plugin type (Enum: "Receiver" | "Storage")
  "EntryPoint": "string"   // The DLL file that contains the plugin implementation
}
```

## File System Structure

Plugins are saved in the file system according to these .NET variables:
```csharp
public static readonly string PluginFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
    "LogMQ", 
    "Plugins"
);

public static readonly string PluginConfigFile = Path.Combine(
    PluginFolder, 
    "pluginconfig.json"
);

public static readonly string PluginBinariesFolder = Path.Combine(
    PluginFolder, 
    "Binaries"
);
```

The plugins will be stored in the `PluginBinariesFolder`, while the schema will be represented in `PluginConfigFile`.

## Plugin Configuration

The `PluginConfigFile` is a JSON file containing a list of objects with this structure:
```json
[
  {
    "Id": "string",                    // A unique identifier for the plugin (GUID)
    "Name": "string",                  // The name of the plugin
    "Type": "string",                  // The plugin type (Enum: "Receiver" | "Storage")
    "Versions": [                      // Array of versions
      {
        "Version": "string",           // The plugin version (e.g., "1.0.0")
        "Status": "string"             // The plugin type status (Enum: "Enabled" | "Disabled" | "Staged" | "Removed")
      }
    ]
  }
]
```

## Plugin Installation

Plugins are pre-installed through the graphical viewer or via the console (CLI project) using the APIs of the shared library `LogMQ.Services.Shared.PluginInstaller`. Initially, plugins are pre-installed with the status `Staged`.

- Plugins are fully installed only after restarting the broker, which updates their status to `Enabled` in the configuration.
- Plugins are fully uninstalled only after restarting the broker, which removes all plugins with the status `Removed` and updates the configuration.

## Plugin Management Commands

### Install a Plugin
Installs a `.lmqex` plugin:
```bash
logmq plugin install <plugin.lmqex> [-r|--restart]
```
- **`<plugin.lmqex>`**: Path to the plugin package.
- **`-r|--restart`**: Installs the plugin and restarts the broker immediately.

### Enable a Plugin
Enables a plugin by name or ID. Optionally, a specific version can be enabled:
```bash
logmq plugin enable <pluginName | pluginId> [-v|--version <version>]
```
- **`<pluginName | pluginId>`**: Name or ID of the plugin.
- **`-v|--version <version>`**: Enables a specific version of the plugin.

### Disable a Plugin
Disables a plugin by name or ID. Optionally, a specific version can be disabled:
```bash
logmq plugin disable <pluginName | pluginId> [-v|--version <version>]
```
- **`<pluginName | pluginId>`**: Name or ID of the plugin.
- **`-v|--version <version>`**: Disables a specific version of the plugin.

### Uninstall a Plugin
Uninstalls a plugin by name or ID. Optionally, a specific version can be uninstalled:
```bash
logmq plugin uninstall <pluginName | pluginId> [-v|--version <version>] [-r|--restart]
```
- **`<pluginName | pluginId>`**: Name or ID of the plugin.
- **`-v|--version <version>`**: Uninstalls a specific version of the plugin.
- **`-r|--restart`**: Forces uninstallation and restarts the broker immediately.

### List Plugins
Lists plugins with optional filters:
```bash
logmq plugin list [-e|--enabled | -d|--disabled | -s|--staged | -r|--removed] [-t|--type <type>]
```
- **`-e|--enabled`**: Filters plugins by _enabled_ status.
- **`-d|--disabled`**: Filters plugins by _disabled_ status.
- **`-s|--staged`**: Filters plugins by _staged_ status.
- **`-r|--removed`**: Filters plugins by _removed_ status.
- **`--type <type>`**: Filters plugins by type (`Receiver`, `Storage`).

### Reset Plugin Configuration
Resets the plugin configuration file based on the current binaries folder:
```bash
logmq plugin reset
```

### Get Plugin Information
Retrieves information about a plugin:
```bash
logmq plugin info <pluginName | pluginId | path/to/plugin.lmqex>
```

---

## Broker Management Commands

### Start the Broker
Starts the LogMQ broker:
```bash
logmq broker start
```

### Stop the Broker
Stops the LogMQ broker:
```bash
logmq broker stop
```

### Restart the Broker
Restarts the LogMQ broker:
```bash
logmq broker restart
```

### Check Broker Status
Checks the status of the LogMQ broker:
```bash
logmq broker status
```

---

## Global Options

- **`-h|--help`**: Displays the help message for any command.
- **`-y`**: Skips confirmation prompts for actions requiring user confirmation.

## Plugin State Restoration

If a plugin's state is modified, it can be reverted to its previous state without consequences, provided the broker has not been restarted.

## Viewer

All plugin management actions are also available through the graphical viewer.

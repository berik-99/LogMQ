# Plugin Management in LogMQ

## Plugin Format

Plugins will have the `.lmqex` extension. The `.lmqex` format is a zip file containing:
- A `manifest.json` file that includes all the plugin metadata.
- A DLL file that must implement the abstract class `LogMQReceiverBase`.

## Manifest Structure

The `manifest.json` file must follow this structure:
```
{
  "Id": "string",          // A unique identifier for the plugin (GUID)
  "Name": "string",        // The name of the plugin
  "Version": "string",     // The plugin version (e.g., "1.0.0")
  "Author": "string",      // The plugin author
  "Description": "string", // A short description of the plugin
  "Type": "string",        // The plugin type (e.g., "Receiver" or "Storage")
  "EntryPoint": "string"   // The DLL file that contains the plugin implementation
}
```

## File System Structure

Plugins will be saved in the file system according to these .NET variables:
```
public static readonly string PluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogMQ", "Plugins");
public static readonly string PluginConfigFile = Path.Combine(PluginFolder, "pluginconfig.json");
public static readonly string PluginBinariesFolder = Path.Combine(PluginFolder, "Binaries");
```

The plugins will be saved in the `PluginBinariesFolder`, while the schema will be represented in `PluginConfigFile`.

## Plugin Configuration

The `PluginConfigFile` is a JSON file containing a list of objects structured as follows:
```
[
    {
        "Id": "string",                    // Guid
        "Name": "string",                  // Plugin name
        "Type": "string",                  // Enum: "Receiver" | "Storage"
        "Versions": [                      // Array of versions
        {
            "Version": "string",           // Version (e.g., "1.0.0")
            "Status": "string"             // Enum: "Enabled" | "Disabled" | "Enlisted" | "Delisted"
        }
        ]
    }
]
```

## Plugin Installation

Pre-installation of plugins will occur through the graphical interface in the viewer or via the console (CLI project), which will use the APIs of the shared library `LogMQ.Services.Shared.PluginInstaller`. Plugins will be pre-installed with the status `Enlisted`.

Plugins will be effectively installed only after restarting the broker, which will handle loading them and updating their status to `Enabled` in the configuration.

Plugins will be effectively uninstalled only after restarting the broker, which will handle removing all plugins with the status `Delisted` and updating the configuration.

## Plugin Management Commands

### Install a Plugin
Installs a `.lmqex` plugin.

```
logmq plugin install <plugin.lmqex> [--force]
```
- **`<plugin.lmqex>`**: Path to the plugin package.
- **`--force`**: Forces installation and restarts the broker immediately.

### Enable a Plugin
Enables a plugin by name or ID. Optionally, enable a specific version.

```
logmq plugin enable <pluginName | pluginId> [--version <version>]
```
- **`<pluginName | pluginId>`**: Name or ID of the plugin.
- **`--version <version>`**: Enables a specific version of the plugin.

### Disable a Plugin
Disables a plugin by name or ID. Optionally, disable a specific version.

```
logmq plugin disable <pluginName | pluginId> [--version <version>]
```
- **`<pluginName | pluginId>`**: Name or ID of the plugin.
- **`--version <version>`**: Disables a specific version of the plugin.

### Uninstall a Plugin
Uninstalls a plugin by name or ID. Optionally, uninstall a specific version.

```
logmq plugin uninstall <pluginName | pluginId> [--version <version>] [--force]
```
- **`<pluginName | pluginId>`**: Name or ID of the plugin.
- **`--version <version>`**: Uninstalls a specific version of the plugin.
- **`--force`**: Forces uninstallation and restarts the broker immediately.

### List Plugins
Lists plugins with optional filters.

```
logmq plugin list [--enabled | --disabled | --type <type>]
```
- **`--enabled`**: Shows only enabled plugins.
- **`--disabled`**: Shows only disabled plugins.
- **`--type <type>`**: Filters plugins by type (e.g., `Receiver`, `Storage`).

### Reset Plugin Configuration
Resets the plugin configuration file based on the current binaries folder.

```
logmq plugin reset-config [--dry-run]
```
- **`--dry-run`**: Simulates the reset and displays a report without applying changes.

---

## Broker Management Commands

### Restart the Broker
Restarts the LogMQ broker.

```
logmq broker restart
```

### Check Broker Status
Checks the status of the LogMQ broker.

```
logmq broker status
```

---

## Logging Commands

### View Logs
Displays the logs of the LogMQ system.

```
logmq logs [--tail] [--verbose]
```
- **`--tail`**: Continuously displays the latest logs.
- **`--verbose`**: Provides detailed log output.

---

## Configuration Management Commands

### Backup Configuration
Backs up the plugin configuration file to the specified path.

```
logmq config backup <path>
```
- **`<path>`**: Destination path for the backup file.

### Restore Configuration
Restores the plugin configuration file from a specified backup.

```
logmq config restore <path>
```
- **`<path>`**: Path to the backup file.

---

## Global Options

- **`--help`**: Displays the help message for any command.
- **`--verbose`**: Provides detailed output for commands.
- **`-y`**: Skips confirmation prompts for all actions that require user confirmation.

## Plugin State Restoration

If a plugin's state has been changed, it can be reverted to its previous state without consequences with the respective action, provided the broker has not been restarted yet.

## Viewer

The same actions can be executed through the viewer.

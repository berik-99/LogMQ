# This is a playground where you can write notes, ideas and other useful things about the LogMQ project

This document serves as a free-form space to:
- Brainstorm new features and improvements
- Document technical decisions and architecture choices
- Keep track of important notes and reminders
- Collect feedback and suggestions
- Draft preliminary designs and concepts

Feel free to use this space to organize thoughts and planning related to LogMQ development.


## New LogMQ commandline descriptions:

### Plugin

```bash
# Install a plugin from his path. Returns info about installed plugin.
#   -e|--enable => Automatically enable installed plugin (if not provided is  prompted;  if there's also -y flag new install and update are non-critical, other cases are critical).
#   -r|--restart => Automatically restart the broker (not overrided by -y flag).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq plugin install <path/to/file.lmqex> [-e|--enable] [-r|--restart] [-y] 

# Enable a plugin from his name or guid; Returns info about enabled plugin.
#   -v|--version => The version to enable, by default enable latest version.
#   -r|--restart => Automatically restart the broker (not overrided by -y flag).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq plugin enable <plugin-name/plugin-id> [-v|--version <plugin-version>] [-r|--restart] [-y] 

# Disable the enabled version of plugin from his name or guid. Returns info about disabled plugin.
#   -r|--restart => Automatically restart the broker (not overrided by -y flag).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq plugin disable <plugin-name/plugin-id> [-r|--restart] [-y]

# Uninstall a plugin from his name or guid; Returns info about uninstalled plugin. (can be reverted with restore command if broker is not yet restarted)
#   -v|--version => The version to uninstall, if not provided a multi-selection prompt will be shown.
#   -r|--restart => Automatically restart the broker (not overrided by -y flag).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq plugin uninstall <plugin-name/plugin-id> [-v|--version <plugin-version>] [-y]

# Show the plugins list.
#   -c|--config-type => Select the configuration to show (Enum: Staged|Running; defaults to Staged).
#   -t|--plugin-type => Filter plugins by type (Enum: Receiver|Storage; if not provided all are displayed).
#   -a|--active => Show only active versions for each plugin, if plugin has no active versions is not displayed. 
logmq plugin list <plugin-name/plugin-id> [-c|--config-type <config-type>] [-t|--plugin-type <plugin-type>] [-a|--active] 

# Show information about provided plugin.
logmq plugin info <plugin-name/plugin-id/path/to/file.lmqex>

# Restore plugin configurations from running config; Running config is compiled  from staged one at broker startup and represents the current broker configuration.
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq plugin restore [-y]
```

### Service

```bash
# Start broker and viewer services. if already started don't do anything.
#   -c|--config-type => Select the configuration to show (Enum: Staged|Running; defaults to Staged).
#   -s|--services => Specify services to start (comma separated list); by default start all services (Enum: Broker, Viewer).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq service start [-c|--config-type <config-type>] [-s|--services <service-a,service-b>] [-y]

# Restart broker and viewer services. 
#   -c|--config-type => Select the configuration to show (Enum: Staged|Running; defaults to Staged).
#   -s|--services => Specify services to restart (comma separated list); by default restart only broker service (Enum: Broker, Viewer).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq service restart [-c|--config-type <config-type>] [-s|--services <service-a,service-b>] [-y] 

# Stop the broker service.
#   -s|--services => Specify services to stop (comma separated list); by default start all services (Enum: Broker, Viewer).
#   -y => Automatically responds yes to non-critical confirmation prompts (critical ones will abort operation).
logmq service stop [-s|--services <service-a,service-b>] [-y]

# Print the status of LogMQ services.
logmq service status 
```

### Log

```bash
# List all logging-registered applications
logmq log list-applications 

# Show logs from a application identified by name or guid.
#	--count => The number of logs to show, defaults to 100.
#	--from => The datetime from which to show logs, defaults to 1 hour ago.
#	--to => The datetime to which to show logs, defaults to now.
#	--type => The trace type of logs to show, defaults to all. 
logmq log show <app-name|app-id> [--count <log-count>] [--from <datetime-from>] [--to <datetime-from>] [--type <trace-type>]

# Watch logs from application identified by name or guid in real time. CTRL-C to exit.
logmq log watch <app-name|app-id>

# Export logs from a application identified by name or guid to a file.
#	--output => The path to the output file where logs will be written.
#	--count => The number of logs to show, defaults to 100.
#	--from => The datetime from which to show logs, defaults to 1 hour ago.
#	--to => The datetime to which to show logs, defaults to now.
#	--type => The trace type of logs to show, defaults to all.
logmq log export <app-name|app-id> [-o|--output <path/to/output.log>] [--count <log-count>] [--from <datetime-from>] [--to <datetime-from>] [--type <trace-type>]

# Write a small report about logs from a application identified by name or guid
#	--output => The path to the output file where report will be written.
logmq log report <app-name|app-id> [-o|--output <path/to/output-report.pdf>] 

# Clear logs from a application identified by name or guid.
#	--older-than => The datetime from which to clear logs, defaults to 1 hour ago.
logmq log clear <app-name|app-id> [--older-than <datatime-older-than>]

# Clear all logs from all applications.
#	--older-than => The datetime from which to clear logs, defaults to 1 hour ago.
logmq log clear-all [--older-than <datatime-older-than>]
```

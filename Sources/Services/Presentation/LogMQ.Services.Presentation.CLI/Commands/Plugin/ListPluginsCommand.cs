using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class ListPluginsCommand : Command<ListPluginsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-e|--enabled")]
        [Description("Show only enabled plugins.")]
        public bool Enabled { get; set; }

        [CommandOption("-d|--disabled")]
        [Description("Show only disabled plugins.")]
        public bool Disabled { get; set; }

        [CommandOption("-s|--staged")]
        [Description("Show only plugins in the 'staged' state.")]
        public bool Staged { get; set; }

        [CommandOption("-r|--removed")]
        [Description("Show only removed plugins.")]
        public bool Removed { get; set; }

        [CommandOption("-t|--type")]
        [Description("Show plugins of the specified type.")]
        public PluginType Type { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        // Logic to determine which filters to apply
        var filters = new string[4];

        if (settings.Enabled) filters[0] = "Enabled";
        if (settings.Disabled) filters[1] = "Disabled";
        if (settings.Staged) filters[2] = "Staged";
        if (settings.Removed) filters[3] = "Removed";

        var activeFilters = filters.Where(f => f != null).ToArray();

        if (activeFilters.Length == 0)
        {
            AnsiConsole.Markup("[yellow]No filters applied, all plugins will be shown.[/]\n");
        }
        else
        {
            AnsiConsole.Markup($"[green]Active filters:[/] {string.Join(", ", activeFilters)}\n");
        }

        // Simulated plugin list
        var pluginList = new[] { "Plugin1", "Plugin2", "Plugin3" };

        // Display the plugin list
        AnsiConsole.Markup("[blue]Plugins:[/]\n");
        foreach (var plugin in pluginList)
        {
            AnsiConsole.Markup($"[cyan]{plugin}[/]\n");
        }

        return 0;
    }
}

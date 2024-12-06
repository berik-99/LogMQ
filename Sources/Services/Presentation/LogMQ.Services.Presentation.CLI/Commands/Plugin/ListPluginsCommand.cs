using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class ListPluginsCommand(IPluginManager manager) : AsyncCommand<ListPluginsCommand.Settings>
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

	public class Settings : CommandSettings
	{
		[CommandOption("-i|--installed")]
		[Description("Show only installed plugins.")]
		public bool Installed { get; set; }

		[CommandOption("-s|--staged")]
		[Description("Show only plugins in the 'staged' state.")]
		public bool Staged { get; set; }

		[CommandOption("-r|--removed")]
		[Description("Show only removed plugins.")]
		public bool Removed { get; set; }

		[CommandOption("-t|--type")]
		[Description("Show plugins of the specified type.")]
		public PluginType? Type { get; set; }

		[CommandOption("-c|--config-type")]
		[Description("Show plugins from specific configuration (Defaults to Staged).")]
		public PluginConfigType ConfigType { get; set; } = PluginConfigType.Staged;
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		List<PluginVersionStatus> filters = [];
		if (settings.Installed) filters.Add(PluginVersionStatus.Installed);
		if (settings.Staged) filters.Add(PluginVersionStatus.Staged);
		if (settings.Removed) filters.Add(PluginVersionStatus.Removed);
		var plugins = await manager.ListPluginsAsync(settings.ConfigType, filters, settings.Type);

		if (plugins.Count > 0)
		{
			var tree = new Tree("[green]Plugins[/]");
			foreach (var plugin in plugins)
			{
				var pluginNode = tree.AddNode($"[blue]{plugin.Name}[/]");
				pluginNode.AddNode($"[yellow]Id:[/] {plugin.Id}");
				pluginNode.AddNode($"[yellow]Author:[/] {plugin.Author}");
				pluginNode.AddNode($"[yellow]Description:[/] {plugin.Description}");
				pluginNode.AddNode($"[yellow]Type:[/] {plugin.Type}");
				pluginNode.AddNode($"[yellow]EntryPoint:[/] {plugin.EntryPoint}");

				var versionsNode = pluginNode.AddNode("[green]Versions[/]");
				foreach (var version in plugin.Versions)
				{
					if (version == plugin.ActiveVersion)
					{
						versionsNode.AddNode($"* Version: [green]{version.Version}[/], Status: [magenta]{version.Status}[/]");
					}
					else
					{
						versionsNode.AddNode($"  Version: [cyan]{version.Version}[/], Status: [magenta]{version.Status}[/]");
					}
				}
			}
			AnsiConsole.Write(tree);
		}
		else
		{
			AnsiConsole.MarkupLine("[red]No plugins found.[/]");
		}
		return 0;
	}
}

using LogMQ.Services.Shared.PluginManager;
using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;

public class InstallPluginCommand(IPluginManager manager) : AsyncCommand<InstallPluginCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<PLUGIN_PATH>")]
		[Description("The path of the .lmqex plugin file.")]
		public string PluginPath { get; set; }

		[CommandOption("-r|--restart")]
		[Description("Automatically restarts the broker after install.")]
		public bool Restart { get; set; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var plugin = await manager.InstallPluginAsync(settings.PluginPath);

		AnsiConsole.MarkupLine($"[green]Plugin '{plugin.Name}' installed successfully![/]");

		var pluginTree = new Tree($"[blue]{plugin.Name}[/]");
		pluginTree.AddNode($"[yellow]Id:[/] {plugin.Id}");
		pluginTree.AddNode($"[yellow]Author:[/] {plugin.Author}");
		pluginTree.AddNode($"[yellow]Description:[/] {plugin.Description}");
		pluginTree.AddNode($"[yellow]Type:[/] {plugin.Type}");
		pluginTree.AddNode($"[yellow]EntryPoint:[/] {plugin.EntryPoint}");

		var versionsNode = pluginTree.AddNode("[green]Versions[/]");
		foreach (var version in plugin.Versions)
		{
			versionsNode.AddNode($"Version: [cyan]{version.Version}[/], Status: [magenta]{version.Status}[/]");
		}
		AnsiConsole.Write(pluginTree);

		//TODO: Use shared methods to format output 
		//TODO: Implement the restart logic

		return 0;
	}
}

using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;

namespace LogMQ.Services.Presentation.CLI.Commands;

internal static class CommonCommands
{
    public static Tree BuildPluginListTree(List<PluginConfig> plugins)
    {
        if (plugins.Count > 0)
        {
            var pluginListTree = new Tree("[green]Plugins[/]");
            foreach (var plugin in plugins)
                pluginListTree.AddNode(BuildPluginTree(plugin));
            return pluginListTree;
        }
        return null;
    }

    public static Tree BuildPluginTree(PluginConfig pluginConfig)
    {
        var pluginTree = new Tree($"[blue]{pluginConfig.Name}[/]");
        pluginTree.AddNode($"[yellow]Id:[/] {pluginConfig.Id}");
        pluginTree.AddNode($"[yellow]Author:[/] {pluginConfig.Author}");
        pluginTree.AddNode($"[yellow]Description:[/] {pluginConfig.Description}");
        pluginTree.AddNode($"[yellow]Type:[/] {pluginConfig.Type}");
        pluginTree.AddNode($"[yellow]Last change:[/] {pluginConfig.LastChangeDate}");
        pluginTree.AddNode($"[yellow]Entry point:[/] {pluginConfig.EntryPoint}");

        var versionsNode = pluginTree.AddNode(pluginConfig.CurrentVersion == null ? "[cyan]Versions[/]" : "[green]Versions[/]");

        foreach (var version in pluginConfig.Versions)
        {
            string color = "cyan";
            string post = "   ";
            string pre = "[[ ]]";
            if (version.Status == VersionStatus.Added)
            {
                post = "(+)";
                color = "yellow";
            }
            if (pluginConfig.CurrentVersion == version.Version)
            {
                pre = "[[*]]";
                color = "green";
            }
            if (version.Status == VersionStatus.Removed)
            {
                post = "(-)";
                color = "red";
            }
            string markup = $"[{color}]{pre} {version.Version} {post} {(version.InsallDate == default ? "" : $"@{version.InsallDate}")}[/]";
            versionsNode.AddNode(markup);
        }
        return pluginTree;
    }

    public static Tree BuildPluginTree(PluginManifest pluginConfig)
    {
        var pluginTree = new Tree($"[blue]{pluginConfig.Name}[/]");
        pluginTree.AddNode($"[yellow]Id:[/] {pluginConfig.Id}");
        pluginTree.AddNode($"[yellow]Author:[/] {pluginConfig.Author}");
        pluginTree.AddNode($"[yellow]Description:[/] {pluginConfig.Description}");
        pluginTree.AddNode($"[yellow]Type:[/] {pluginConfig.Type}");
        pluginTree.AddNode($"[yellow]Last change:[/] NOT INSTALLED");
        pluginTree.AddNode($"[yellow]Entry point:[/] {pluginConfig.EntryPoint}");
        var versionsNode = pluginTree.AddNode("[cyan]Versions[/]");
        versionsNode.AddNode($"[cyan] {pluginConfig.CurrentVersion} [/]");
        return pluginTree;
    }
}

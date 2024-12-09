using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;

namespace LogMQ.Services.Presentation.CLI.Commands;

internal static class CommonCommands
{
    //public static Tree BuildPluginTree(PluginConfig plugin, List<Version> added = null, List<Version> removed = null)
    //{
    //    var pluginTree = new Tree($"[blue]{plugin.Name}[/]");
    //    pluginTree.AddNode($"[yellow]Id:[/] {plugin.Id}");
    //    pluginTree.AddNode($"[yellow]Author:[/] {plugin.Author}");
    //    pluginTree.AddNode($"[yellow]Description:[/] {plugin.Description}");
    //    pluginTree.AddNode($"[yellow]Type:[/] {plugin.Type}");
    //    pluginTree.AddNode($"[yellow]EntryPoint:[/] {plugin.EntryPoint}");

    //    var versionsNode = pluginTree.AddNode(plugin.EnabledVersion == null ? "[cyan]Versions[/]" : "[green]Versions[/]");

    //    foreach (var version in plugin.Versions)
    //        versionsNode.AddNode(plugin.EnabledVersion == version ? $"*[green]{version}[/]" : $" [cyan]{version}[/]");


    //    return pluginTree;
    //}

    public static Tree BuildPluginTree(PluginConfig plugin, List<Version> added = null, List<Version> removed = null)
    {
        removed ??= [];
        added ??= [];
        var pluginTree = new Tree($"[blue]{plugin.Name}[/]");
        pluginTree.AddNode($"[yellow]Id:[/] {plugin.Id}");
        pluginTree.AddNode($"[yellow]Author:[/] {plugin.Author}");
        pluginTree.AddNode($"[yellow]Description:[/] {plugin.Description}");
        pluginTree.AddNode($"[yellow]Type:[/] {plugin.Type}");
        pluginTree.AddNode($"[yellow]EntryPoint:[/] {plugin.EntryPoint}");

        var versionsNode = pluginTree.AddNode(plugin.EnabledVersion == null ? "[cyan]Versions[/]" : "[green]Versions[/]");

        var versions = plugin.Versions.ToList();
        versions.AddRange(removed);
        versions = [.. versions.OrderDescending()];

        foreach (var version in versions)
        {
            string color = "cyan";
            string prefix = " ";
            string postfix = "[[ ]]";
            if (plugin.EnabledVersion == version)
            {
                postfix = "[[*]]";
                color = "green";
            }
            if (added.Contains(version))
            {
                prefix = "+";
                color = "yellow";
            }
            if (removed.Contains(version))
            {
                prefix = "-";
                color = "red";
            }
            string markup = $"[{color}]{postfix} {version} {prefix}[/]";
            versionsNode.AddNode(markup);
        }
        return pluginTree;
    }
}

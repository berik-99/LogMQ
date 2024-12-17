using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;

namespace LogMQ.Services.Presentation.CLI.Commands;

internal static class CommonCommands
{
    public static void ShowPluginListTree(List<PluginConfig> plugins)
    {
        if (plugins.Count > 0)
        {
            var pluginListTree = new Tree("[green]Plugins[/]");
            foreach (var plugin in plugins)
                pluginListTree.AddNode(BuildPluginTree(plugin));
            AnsiConsole.Write(pluginListTree);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No plugins found.[/]");
        }
    }

    public static void ShowPluginTree(PluginConfig pluginConfig, List<Version> otherVerison = null)
    {
        AnsiConsole.Write(BuildPluginTree(pluginConfig, otherVerison));
    }

    public static bool ConfirmOperation(bool autoConfirm, string message)
    {
        if (autoConfirm)
        {
            return true;
        }

        var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>(message)
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(true)
            .WithConverter(choice => choice ? "y" : "n"));

        if (!confirmation)
        {
            AnsiConsole.MarkupLine("[red]Operation aborted by user.[/]");
        }

        return confirmation;
    }

    private static Tree BuildPluginTree(PluginConfig pluginConfig, List<Version> otherVerison = null)
    {
        otherVerison ??= [];
        var pluginTree = new Tree($"[blue]{pluginConfig.Id}[/]");
        pluginTree.AddNode($"[yellow]Name:[/] {pluginConfig.Name}");
        pluginTree.AddNode($"[yellow]Author:[/] {pluginConfig.Author}");
        pluginTree.AddNode($"[yellow]Description:[/] {pluginConfig.Description}");
        pluginTree.AddNode($"[yellow]Type:[/] {pluginConfig.Type}");
        pluginTree.AddNode($"[yellow]Last change:[/] {pluginConfig.LastChangeDate}");
        pluginTree.AddNode($"[yellow]Entry point:[/] {pluginConfig.EntryPoint}");

        var versionsNode = pluginTree.AddNode(pluginConfig.CurrentVersion == null ? "[cyan]Versions[/]" : "[green]Versions[/]");

        HashSet<ConfigVersion> mergedVersions = new(pluginConfig.Versions);
        foreach (var v in otherVerison)
            mergedVersions.Add(new ConfigVersion { Version = v });

        foreach (var version in mergedVersions)
        {
            string color = "cyan";
            string post = "   ";
            string pre = "[[ ]]";
            string installDate = version.InsallDate == default ? "NO DATE" : version.InsallDate.ToString();
            if (version.IsAdded)
            {
                post = "(+)";
                color = "yellow";
            }
            if (pluginConfig.CurrentVersion == version.Version)
            {
                pre = "[[*]]";
                color = "green";
            }
            if (version.IsRemoved)
            {
                post = "(-)";
                color = "red";
            }
            if (!pluginConfig.Versions.Contains(version) && otherVerison.Contains(version.Version))
            {
                post = "(X)";
                color = "white";
                installDate = "NOT INSTALLED";
            }
            string markup = $"[{color}]{pre} {version.Version} {post} INSTALLED: {installDate}[/]";
            versionsNode.AddNode(markup);
        }
        return pluginTree;
    }
}

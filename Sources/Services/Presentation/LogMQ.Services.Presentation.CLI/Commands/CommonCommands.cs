using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;

namespace LogMQ.Services.Presentation.CLI.Commands;

internal static class CommonCommands
{
    public static void ShowPluginListTree(Dictionary<PluginConfig, Version> plugins)
    {
        if (plugins.Count > 0)
        {
            var pluginListTree = new Tree("[green]Plugins[/]");
            foreach (var plugin in plugins)
                pluginListTree.AddNode(BuildPluginTree(plugin.Key, plugin.Value, null));
            AnsiConsole.Write(pluginListTree);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No plugins found.[/]");
        }
    }

    public static void ShowPluginTree(PluginConfig pluginConfig, Version running, List<Version> otherVerison = null)
    {
        AnsiConsole.Write(BuildPluginTree(pluginConfig, running, otherVerison));
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

    private static Tree BuildPluginTree(PluginConfig pluginConfig, Version running, List<Version> otherVerison)
    {
        otherVerison ??= [];
        var pluginTree = new Tree($"[blue]{pluginConfig.Id}[/]");
        pluginTree.AddNode($"[yellow]Name:[/] {pluginConfig.Name}");
        pluginTree.AddNode($"[yellow]Author:[/] {pluginConfig.Author}");
        pluginTree.AddNode($"[yellow]Description:[/] {pluginConfig.Description}");
        pluginTree.AddNode($"[yellow]Type:[/] {pluginConfig.Type}");
        pluginTree.AddNode($"[yellow]Entry point:[/] {pluginConfig.EntryPoint}");

        var versionsNode = pluginTree.AddNode("[yellow]Versions[/]");

        HashSet<PluginVersion> mergedVersions = new(pluginConfig.Versions);
        foreach (var v in otherVerison)
            mergedVersions.Add(new PluginVersion { Version = v });

        if (mergedVersions.Count > 0)
        {
            foreach (var version in mergedVersions)
            {
                string color = "cyan";
                string status = "INSTALLED";
                switch (version.Status)
                {
                    case VersionStatus.Installed:
                        if (version.Version == running)
                        {
                            color = "yellow4_1";
                            status = "DISABLED";
                        }
                        else
                        {
                            color = "cyan";
                            status = "INSTALLED";
                        }
                        break;
                    case VersionStatus.Enabled:
                        if (version.Version == running)
                        {
                            color = "green";
                            status = "RUNNING";
                        }
                        else
                        {
                            color = "yellow";
                            status = "ENABLED";
                        }
                        break;
                    case VersionStatus.Removed:
                        color = "red";
                        status = "REMOVED";
                        break;
                }
                if (!pluginConfig.Versions.Contains(version) && otherVerison.Contains(version.Version))
                {
                    color = "white";
                    status = "NOT INSTALLED";
                }
                string markup = $"[{color}]{version.Version} - STATUS: {status}[/]";
                versionsNode.AddNode(markup);
            }
        }
        else
        {
            versionsNode.AddNode("[red]No versions installed[/]");
        }
        return pluginTree;
    }
}

using LogMQ.Services.Shared.PluginManager.Models;
using Spectre.Console;

namespace LogMQ.Services.Presentation.CLI.Commands;

/// <summary>
/// Contains common commands and utility methods for displaying plugin information in the CLI.
/// </summary>
internal static class CommonCommands
{
    /// <summary>
    /// Displays a tree of plugins and their details in the console.
    /// </summary>
    /// <param name="plugins">A dictionary containing plugin configurations and their current running versions.</param>
    public static void ShowPluginListTree(Dictionary<PluginConfig, Version> plugins)
    {
        if (plugins.Count > 0)
        {
            Tree pluginListTree = new("[green]Plugins[/]");
            foreach (KeyValuePair<PluginConfig, Version> plugin in plugins)
                pluginListTree.AddNode(BuildPluginTree(plugin.Key, plugin.Value, null));
            AnsiConsole.Write(pluginListTree);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No plugins found.[/]");
        }
    }

    /// <summary>
    /// Displays detailed information about a single plugin in a tree format.
    /// </summary>
    /// <param name="pluginConfig">The configuration of the plugin to display.</param>
    /// <param name="running">The currently running version of the plugin.</param>
    /// <param name="otherVerison">A list of other versions of the plugin, if any.</param>
    public static void ShowPluginTree(PluginConfig pluginConfig, Version running, List<Version> otherVerison = null)
    {
        AnsiConsole.Write(BuildPluginTree(pluginConfig, running, otherVerison));
    }

    /// <summary>
    /// Prompts the user for confirmation before proceeding with an operation.
    /// </summary>
    /// <param name="autoConfirm">If true, automatically confirms the operation without prompting the user.</param>
    /// <param name="message">The confirmation message to display to the user.</param>
    /// <returns>True if the operation is confirmed, false otherwise.</returns>
    public static bool ConfirmOperation(bool autoConfirm, string message)
    {
        if (autoConfirm)
        {
            return true;
        }

        bool confirmation = AnsiConsole.Prompt(new TextPrompt<bool>(message)
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

    /// <summary>
    /// Builds a tree structure representing the details of a plugin, including its versions and status.
    /// </summary>
    /// <param name="pluginConfig">The configuration of the plugin.</param>
    /// <param name="running">The currently running version of the plugin.</param>
    /// <param name="externalVersions">A list of other versions of the plugin, if any.</param>
    /// <returns>A tree structure representing the plugin details.</returns>
    private static Tree BuildPluginTree(PluginConfig pluginConfig, Version running, List<Version> externalVersions)
    {
        externalVersions ??= [];
        Tree pluginTree = new($"[blue]{pluginConfig.Id}[/]");
        pluginTree.AddNode($"[yellow]Name:[/] {pluginConfig.Name}");
        pluginTree.AddNode($"[yellow]Author:[/] {pluginConfig.Author}");
        pluginTree.AddNode($"[yellow]Description:[/] {pluginConfig.Description}");
        pluginTree.AddNode($"[yellow]Type:[/] {pluginConfig.Type}");
        pluginTree.AddNode($"[yellow]Entry point:[/] {pluginConfig.EntryPoint}");

        TreeNode versionsNode = pluginTree.AddNode("[yellow]Versions[/]");

        HashSet<PluginVersion> mergedVersions = new(pluginConfig.Versions);
        foreach (Version v in externalVersions)
            mergedVersions.Add(new PluginVersion { Version = v });

        if (mergedVersions.Count > 0)
        {
            foreach (PluginVersion version in mergedVersions)
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
                if (!pluginConfig.Versions.Contains(version) && externalVersions.Contains(version.Version))
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

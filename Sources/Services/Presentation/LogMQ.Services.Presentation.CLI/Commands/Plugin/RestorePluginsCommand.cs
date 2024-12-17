using LogMQ.Services.Shared.PluginManager;
using Spectre.Console.Cli;
using System.ComponentModel;
using static LogMQ.Services.Presentation.CLI.Commands.CommonCommands;

namespace LogMQ.Services.Presentation.CLI.Commands.Plugin;
public class RestorePluginsCommand(IPluginManager manager) : AsyncCommand<RestorePluginsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-y")]
        [Description("Automatically respond y to all propmts.")]
        public bool Yes { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!ConfirmOperation(settings.Yes, "This operation is not reversible. Do you want to continue?"))
        {
            return -1;
        }

        var plugins = await manager.RestorePluginConfigAsync();
        ShowPluginListTree(plugins);
        return 0;
    }
}
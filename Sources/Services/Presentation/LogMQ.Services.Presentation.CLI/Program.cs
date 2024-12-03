using LogMQ.Services.Presentation.CLI.Commands.Broker;
using LogMQ.Services.Presentation.CLI.Commands.Plugin;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var app = new CommandApp();

        // Comando principale "plugin"
        app.Configure(config =>
        {
            config.AddBranch("plugin", c =>
            {
                c.AddCommand<InstallPluginCommand>("install")
                  .WithDescription("Installs a plugin package.");

                c.AddCommand<EnablePluginCommand>("enable")
                  .WithDescription("Enables a plugin by name or ID.");

                c.AddCommand<DisablePluginCommand>("disable")
                  .WithDescription("Disables a plugin by name or ID.");

                c.AddCommand<UninstallPluginCommand>("uninstall")
                  .WithDescription("Uninstalls a plugin.");

                c.AddCommand<ListPluginsCommand>("list")
                  .WithDescription("Lists all plugins with optional filters.");

                c.AddCommand<ResetPluginsCommand>("reset")
                  .WithDescription("Resets the plugin configuration.");
            });

            // Comandi relativi al broker
            config.AddBranch("broker", c =>
            {
                c.AddCommand<RestartBrokerCommand>("restart")
                      .WithDescription("Restarts the LogMQ broker.");

                c.AddCommand<RestartBrokerCommand>("stop")
                      .WithDescription("Stop the LogMQ broker.");

                c.AddCommand<RestartBrokerCommand>("start")
                      .WithDescription("Start the LogMQ broker.");

                c.AddCommand<BrokerStatusCommand>("status")
                      .WithDescription("Checks the status of the LogMQ broker.");
            });
        });

        await app.RunAsync(args);
    }
}
using System.Runtime.InteropServices;
using LogMQ.Services.Presentation.CLI.Commands.BrokerCommands;
using LogMQ.Services.Presentation.CLI.Commands.LogCommands;
using LogMQ.Services.Presentation.CLI.Commands.PluginCommands;
using LogMQ.Services.Presentation.CLI.DependencyInjection;
using LogMQ.Services.Shared.BrokerManager;
using LogMQ.Services.Shared.PluginManager;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ServiceCollection services = new();

        services.AddSingleton<IPluginManager, PluginManager>();
        Type platformBrokerManager = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? typeof(WindowsBrokerManager) : typeof(LinuxBrokerManager);
        services.AddSingleton(typeof(IBrokerManager), platformBrokerManager);

        TypeRegistrar registrar = new(services);
        CommandApp app = new(registrar);

        // Plugin commands
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

                c.AddCommand<PluginInfoCommand>("info")
                  .WithDescription("Gets the plugin informations.");

                c.AddCommand<RestorePluginsCommand>("restore")
                  .WithDescription("Resets the plugin configuration.");
            });

            // Broker commands
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

            // Log commands
            config.AddBranch("log", c =>
            {
                c.AddCommand<ShowLogsCommand>("show")
                      .WithDescription("Show logs from an application.");

                c.AddCommand<WatchLogsCommand>("watch")
                      .WithDescription("Watch logs from an application.");
            });
        });

        await app.RunAsync(args);
    }
}
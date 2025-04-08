using System.Runtime.InteropServices;

namespace LogMQ.Services.Broker.Worker;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSystemServiceProvider(this IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            services.AddWindowsService();
        else
            services.AddSystemd();
        return services;
    }
}
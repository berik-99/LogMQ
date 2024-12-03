using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    private readonly IServiceProvider provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object Resolve(Type type)
    {
        return provider.GetRequiredService(type);
    }
}

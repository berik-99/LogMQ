using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.DependencyInjection;

internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    private readonly IServiceProvider provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object Resolve(Type type) => provider.GetRequiredService(type);
}

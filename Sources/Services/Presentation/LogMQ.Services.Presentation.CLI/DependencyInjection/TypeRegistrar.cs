using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace LogMQ.Services.Presentation.CLI.DependencyInjection;

internal sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(builder.BuildServiceProvider());

    public void Register(Type service, Type implementation) => builder.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) => builder.AddSingleton(service, factory);
}

using System.Runtime.InteropServices;
using LogMQ.Receivers;
using LogMQ.Services.Broker.Worker;
using LogMQ.Storage;
using LogMQ.Storage.Contracts;
using Serilog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging
   .ClearProviders()
   .AddSerilog(
       new LoggerConfiguration()
           .WriteTo.Console()
           //.WriteTo.File(Path.Join(builder.Environment.ContentRootPath, "myApp.log"))
           .CreateLogger()
   );

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddWindowsService();
else
    builder.Services.AddSystemd();

builder.Services.AddSingleton<ILogStorage, RocksDbStorage>();
builder.Services.AddHostedService<TcpReceiver>();
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddHostedService<MsmqReceiver>();
builder.Services.AddHostedService<TestDbReaderService>();

IHost host = builder.Build();

await host.RunAsync();

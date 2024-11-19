using LogMQ.Broker.Services.BackgrondServices;
using LogMQ.Broker.Services.InternalQueueServices;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging
   .ClearProviders()
   .AddSerilog(
       new LoggerConfiguration()
           .WriteTo.Console()
           //.WriteTo.File(Path.Join(builder.Environment.ContentRootPath, "myApp.log"))
           .CreateLogger()
   );

builder.Services.AddWindowsService();
builder.Services.AddSingleton<RocksDbService>();
builder.Services.AddHostedService<SocketReader>();
builder.Services.AddHostedService<MSMQReader>();

var host = builder.Build();
await host.RunAsync();
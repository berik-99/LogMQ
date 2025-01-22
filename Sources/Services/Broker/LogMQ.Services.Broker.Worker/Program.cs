using System.Runtime.InteropServices;
using LogMQ.Receivers;
using LogMQ.Services.Broker.Worker.Services;
using LogMQ.Services.Shared.LogManager;
using LogMQ.Storage;
using LogMQ.Storage.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(LogMQ.Services.Shared.Common.Defaults.GrpcAddress);
builder.WebHost.ConfigureKestrel(opts => opts.ConfigureEndpointDefaults(static endpoints => endpoints.Protocols = HttpProtocols.Http2));

builder.Logging
   .ClearProviders()
   .AddSerilog(
       new LoggerConfiguration()
           .WriteTo.Console()
           //.WriteTo.File(Path.Join(Defaults.DataFolder, "Logs"))
           .CreateLogger()
   );

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddWindowsService();
else
    builder.Services.AddSystemd();

builder.Services.AddSingleton(new RocksDbStorageConfiguration() { DbPath = Path.Combine(LogMQ.Services.Shared.Common.Defaults.DataFolder, "Data", "RocksDB", "db") });
builder.Services.AddSingleton<ILogStorage, RocksDbStorage>();
builder.Services.AddSingleton<ILogService, RocksDbStorageService>();

builder.Services.AddHostedService<TcpReceiver>();
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddHostedService<MsmqReceiver>();

builder.Services.AddCodeFirstGrpc();

var app = builder.Build();

app.MapGrpcService<ILogService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

await app.RunAsync();

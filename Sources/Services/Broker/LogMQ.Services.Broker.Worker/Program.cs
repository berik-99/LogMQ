using System.Runtime.InteropServices;
using LogMQ.Receivers;
using LogMQ.Receivers.Contracts;
using LogMQ.Services.Broker.Worker.Services;
using LogMQ.Services.Shared.LogManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(Constants.GrpcAddress);
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

builder.Services.AddSingleton(new DuckDBStorageConfiguration() { DatabaseFolderPath = Path.Combine(LogMQ.Services.Shared.Common.Constants.DataFolder, "Data") });
//Registering storage service
builder.Services.AddSingleton<ILogStorage, DuckDBStorageService>();

//Registering gRPC service
builder.Services.AddSingleton<ILogGrpcService, DuckDBStorageService>();

builder.Services.AddHostedService<TcpReceiver>();
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddHostedService<MsmqReceiver>();

builder.Services.AddCodeFirstGrpc();

WebApplication app = builder.Build();

app.MapGrpcService<ILogGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

await app.RunAsync();

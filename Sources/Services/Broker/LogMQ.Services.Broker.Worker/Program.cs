using System.Runtime.InteropServices;
using System.Threading.Channels;
using LogMQ.Core;
using LogMQ.Receivers;
using LogMQ.Receivers.Contracts;
using LogMQ.Services.Broker.Worker;
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

builder.Services.AddSystemServiceProvider();
builder.Services.AddCodeFirstGrpc();

//Repository configuration
builder.Services.AddSingleton(new RepositoryConfiguration() { DatabaseFolderPath = Path.Combine(LogMQ.Services.Shared.Common.Constants.DataFolder, "Data") });
//Message channel
builder.Services.AddSingleton(_ => Channel.CreateUnbounded<LogMessage>());

//Message Writer Progessor 
builder.Services.AddHostedService<MessageWriterProcessor>();

//Registering storage service
builder.Services.AddSingleton<ILogStorage, RepositoryService>();

//Registering gRPC service
builder.Services.AddSingleton<ILogGrpcService, RepositoryService>();

//Registering receivers
builder.Services.AddHostedService<TcpReceiver>();
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddHostedService<MsmqReceiver>();

WebApplication app = builder.Build();

app.MapGrpcService<ILogGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

await app.RunAsync();
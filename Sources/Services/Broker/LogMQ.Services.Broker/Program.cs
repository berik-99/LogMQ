using LogMQ.Receivers;
using LogMQ.Storage;
using LogMQ.Storage.Contracts;
using Serilog;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

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

var host = builder.Build();
await host.RunAsync();
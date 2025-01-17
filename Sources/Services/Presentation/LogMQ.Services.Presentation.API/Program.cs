using LogMQ.Services.Presentation.API.Services;
using LogMQ.Storage;
using LogMQ.Storage.Contracts;
using ProtoBuf.Grpc.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILogStorage, RocksDbStorage>();

// Add services to the container.
builder.Services.AddCodeFirstGrpc();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LogService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();

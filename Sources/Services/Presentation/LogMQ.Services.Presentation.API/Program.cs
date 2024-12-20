using LogMQ.Services.Presentation.API.Services;
using ProtoBuf.Grpc.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCodeFirstGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LogService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

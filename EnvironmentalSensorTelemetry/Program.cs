using EnvironmentalSensorTelemetry.Services;
using app.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection(); // For development purposes (Postman)
builder.Services.AddSingleton<InfluxDBService>(); // For InfluxDBService DI

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

// Configure the HTTP request pipeline.
app.MapGrpcService<EnvironmentalSensorTelemetryService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
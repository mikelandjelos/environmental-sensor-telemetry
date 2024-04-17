using EnvironmentalSensorTelemetry.Services;
using app.Services;
using app.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc().AddServiceOptions<EnvironmentalSensorTelemetryService>(options =>
{
    options.Interceptors.Add<ServerLoggerInterceptor>();
});
builder.Services.AddGrpcReflection(); // For development purposes (Postman)

builder.Services.AddSingleton<InfluxDBApiFactoryService>(); // For InfluxDBService DI
builder.Services.AddSingleton<ServerLoggerInterceptor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

// Configure the HTTP request pipeline.
app.MapGrpcService<EnvironmentalSensorTelemetryService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using app.Services;
using System.Net;

namespace EnvironmentalSensorTelemetry.Services;

public class EnvironmentalSensorTelemetryService : EnvironmentalSensorTelemetry.EnvironmentalSensorTelemetryBase
{
    private readonly ILogger<EnvironmentalSensorTelemetryService> _logger;
    private readonly InfluxDBService _influxDBService;

    public EnvironmentalSensorTelemetryService(ILogger<EnvironmentalSensorTelemetryService> logger, InfluxDBService influxDBService)
    {
        _logger = logger;
        _influxDBService = influxDBService;
    }

    public override async Task<PingResponse> Ping(Empty request, ServerCallContext context)
    {
        _logger.LogInformation($"{context.Peer} called method {context.Method} at {DateTime.UtcNow}");
        try
        {

            var pingResponseSuccess = await _influxDBService.Ping();

            return new PingResponse
            {
                Pong = pingResponseSuccess ? "Pong" : "Failed",
                Metadata = new ResponseMetadata
                {
                    Message = pingResponseSuccess ? "InfluxDB pinged successfully!" : "InfluxDB ping failed!",
                    StatusCode = (int)HttpStatusCode.OK,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
            return new PingResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                }
            };
        }
    }
}

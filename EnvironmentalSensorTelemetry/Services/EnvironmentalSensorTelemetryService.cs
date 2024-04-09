using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using app.Services;
using System.Net;
using app.Models;
using InfluxDB.Client.Api.Domain;

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

    public override Task<WriteDataResponse> WriteMeasurementsBatched(WriteBatchRequest request, ServerCallContext context)
    {
        try
        {
            var measurements = request.Data.Select(estData => new app.Models.EnvironmentalSensorTelemetryData
            {
                Timestamp = DateTime.UtcNow,
                Device = estData.Device,
                CarbonOxide = estData.CarbonOxide,
                Humidity = estData.Humidity,
                Light = estData.Light,
                LiquidPetroleumGas = estData.LiquidPetroleumGas,
                Motion = estData.Motion,
                Smoke = estData.Smoke,
                Temperature = estData.Temperature,
            }).ToList();

            _influxDBService.Write(writeApi =>
            {
                writeApi.WriteMeasurements(measurements, WritePrecision.Ns);
            });

            return Task.FromResult(new WriteDataResponse
            {
                WrittenAt = Timestamp.FromDateTime(DateTime.UtcNow),
                Metadata = new ResponseMetadata
                {
                    Message = $"Successfully written {measurements.Count} instances!",
                    StatusCode = (int)HttpStatusCode.OK,
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
            return Task.FromResult(new WriteDataResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            });
        }
    }

    public override Task WriteMeasurementsStream(IAsyncStreamReader<EnvironmentalSensorTelemetryData> requestStream, IServerStreamWriter<WriteDataResponse> responseStream, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
}

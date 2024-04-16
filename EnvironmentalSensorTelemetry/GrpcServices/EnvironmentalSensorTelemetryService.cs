using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using app.Services;
using System.Net;
using InfluxDB.Client.Api.Domain;
using app.Models;
using System.Reactive.Linq;
using InfluxDB.Client.Core.Exceptions;

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

            if (!pingResponseSuccess)
                _logger.LogError("InfluxDB ping has failed!");

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
            var measurements = request.Data.Select(estData => new SensorData
            {
                Timestamp = estData.Timestamp.ToDateTime(),
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
        try
        {
            _influxDBService.Write(async writeApi =>
            {
                while (await requestStream.MoveNext())
                {
                    var estData = requestStream.Current;
                    writeApi.WriteMeasurement(new SensorData
                    {
                        Timestamp = estData.Timestamp.ToDateTime(),
                        Device = estData.Device,
                        CarbonOxide = estData.CarbonOxide,
                        Humidity = estData.Humidity,
                        Light = estData.Light,
                        LiquidPetroleumGas = estData.LiquidPetroleumGas,
                        Motion = estData.Motion,
                        Smoke = estData.Smoke,
                        Temperature = estData.Temperature,
                    }, WritePrecision.Ns);

                    await responseStream.WriteAsync(new WriteDataResponse
                    {
                        WrittenAt = Timestamp.FromDateTime(DateTime.UtcNow),
                        Metadata = new ResponseMetadata
                        {
                            Message = $"Successfully written instance!",
                            StatusCode = (int)HttpStatusCode.OK,
                        }
                    });
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    public override async Task<ReadMeasurementsOverTimeSpanResponse> ReadMeasurementsOverTimeSpan(ReadMeasurementsOverTimeSpanRequest request, ServerCallContext context)
    {
        try
        {
            // Flux query can be injected with malicious strings - its "SQL" injection prone.
            string stopTime = request.StopTime != null ? $", stop: time(v: {request.StopTime.ToDateTime().ToTimestamp()})" : string.Empty;
            string pagination = request.PaginationInfo != null && request.PaginationInfo.PageSize > 0 && request.PaginationInfo.Offset >= 0 ?
                $"\t|> limit(n: {request.PaginationInfo.PageSize}, offset: {request.PaginationInfo.Offset})\n" : string.Empty;

            var query = string.Join("",
                $"from(bucket: \"{_influxDBService.DefaultBucket}\")\n",
                $"\t|> range(start: time(v: {request.StartTime.ToDateTime().ToTimestamp()}){stopTime})\n",
                $"\t|> pivot(rowKey: [\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                pagination);

            var fluxTables = await _influxDBService.QueryAsync(queryApi => queryApi.QueryAsync(query));
            var records = new List<EnvironmentalSensorTelemetryData>();

            _logger.LogInformation(query);
            _logger.LogInformation(fluxTables.Sum(fluxTable => fluxTable.Records.Count).ToString());

            records = fluxTables.SelectMany(fluxTable =>
                fluxTable.Records.Select(record =>
                    new EnvironmentalSensorTelemetryData
                    {
                        Timestamp = Timestamp.FromDateTime(record.GetTimeInDateTime()!.Value),
                        CarbonOxide = (double)record.GetValueByKey("carbon_oxide"),
                        Device = (string)record.GetValueByKey("device"),
                        Humidity = (double)record.GetValueByKey("humidity"),
                        Light = (bool)record.GetValueByKey("light"),
                        LiquidPetroleumGas = (double)record.GetValueByKey("liquid_petroleum_gas"),
                        Motion = (bool)record.GetValueByKey("motion"),
                        Smoke = (double)record.GetValueByKey("smoke"),
                        Temperature = (double)record.GetValueByKey("temperature"),
                    })).ToList();

            return new ReadMeasurementsOverTimeSpanResponse
            {
                Records = { records },
                Metadata = new ResponseMetadata
                {
                    Message = $"Succesfully read {records.Count} records, from {request.StartTime.ToDateTime()}, to {request.StopTime?.ToDateTime() ?? DateTime.Now}",
                    StatusCode = (int)HttpStatusCode.OK,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
            return new ReadMeasurementsOverTimeSpanResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };
        }
    }

    public override async Task<ReadFieldsOverTimeSpanResponse> ReadFieldsOverTimeSpan(ReadFieldsOverTimeSpanRequest request, ServerCallContext context)
    {
        try
        {
            // Flux query can be injected with malicious strings - its "SQL" injection prone.
            string stopTime = request.StopTime != null ? $", stop: time(v: {request.StopTime.ToDateTime().ToTimestamp()})" : string.Empty;
            string filter = request.HasFieldName ? $"\t|> filter(fn: (r) => r._field == \"{request.FieldName}\")\n" : string.Empty;
            string pagination = request.PaginationInfo != null && request.PaginationInfo.PageSize > 0 && request.PaginationInfo.Offset >= 0 ?
                $"\t|> limit(n: {request.PaginationInfo.PageSize}, offset: {request.PaginationInfo.Offset})\n" : string.Empty;

            var query = string.Join("",
                $"from(bucket: \"{_influxDBService.DefaultBucket}\")\n",
                $"\t|> range(start: time(v: {request.StartTime.ToDateTime().ToTimestamp()}){stopTime})\n",
                filter,
                pagination);

            var fluxTables = await _influxDBService.QueryAsync(queryApi => queryApi.QueryAsync(query));
            var records = new List<FluxRecordCompact>();

            records = fluxTables.SelectMany(fluxTable =>
                fluxTable.Records.Select(record =>
                    new FluxRecordCompact
                    {
                        FieldName = record.GetField(),
                        Timestamp = Timestamp.FromDateTime(record.GetTimeInDateTime()!.Value),
                        Value = record.GetValue().GetType() == typeof(bool) ?
                            Value.ForBool((bool)record.GetValue()) :
                            Value.ForNumber((double)record.GetValue())
                    })).ToList();

            return new ReadFieldsOverTimeSpanResponse
            {
                Records = { records },
                Metadata = new ResponseMetadata
                {
                    Message = $"Succesfully read {records.Count} records, from {request.StartTime.ToDateTime()}, to {request.StopTime?.ToDateTime() ?? DateTime.Now}",
                    StatusCode = (int)HttpStatusCode.OK,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
            return new ReadFieldsOverTimeSpanResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };
        }
    }

    public override Task<AggregateOverTimeSpanResponse> AggregateOverTimeSpan(AggregateOverTimeSpanRequest request, ServerCallContext context)
    {
        throw new MethodNotAllowedException("AggregateOverTimeSpan");
    }
}

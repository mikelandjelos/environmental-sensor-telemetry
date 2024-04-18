using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using app.Services;
using System.Net;
using InfluxDB.Client.Api.Domain;
using app.Models;
using System.Reactive.Linq;

namespace EnvironmentalSensorTelemetry.Services;

public class EnvironmentalSensorTelemetryService : EnvironmentalSensorTelemetry.EnvironmentalSensorTelemetryBase
{
    private readonly ILogger<EnvironmentalSensorTelemetryService> _logger;
    private readonly InfluxDBApiFactoryService _influxDBService;

    public EnvironmentalSensorTelemetryService(ILogger<EnvironmentalSensorTelemetryService> logger, InfluxDBApiFactoryService influxDBService)
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
                    StatusCode = pingResponseSuccess ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound,
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
                while (!context.CancellationToken.IsCancellationRequested && await requestStream.MoveNext())
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

    /// <summary>
    /// - Example query:
    /// 
    /// ```flux
    ///  from(bucket: "EnvironmentalSensorTelemetry")
    ///      |> range(start: time(v: "2024-04-15T10:57:40.637Z"), stop: time(v: "2024-04-16T10:57:47.652Z"))
    ///      |> pivot(rowKey: ["_time"], columnKey: ["_field"], valueColumn: "_value")	
    ///      |> limit(n: 2, offset: 1)
    /// ```
    /// - Example response:
    /// 
    /// ```json
    ///  {
    ///      "records": [
    ///          {
    ///              "timestamp": "2024-04-16T07:26:25.828Z",
    ///              "device": "c8:94:02:0e:fd:97",
    ///              "carbon_oxide": 0.1,
    ///              "humidity": 0.1,
    ///              "light": true,
    ///              "liquid_petroleum_gas": 0.1,
    ///              "motion": true,
    ///              "smoke": 0.1,
    ///              "temperature": 0.1
    ///          },
    ///          {
    ///              "timestamp": "2024-04-16T07:26:56.625Z",
    ///              "device": "c8:94:02:0e:fd:97",
    ///              "carbon_oxide": 0.11,
    ///              "humidity": 0.11,
    ///              "light": true,
    ///              "liquid_petroleum_gas": 0.11,
    ///              "motion": true,
    ///              "smoke": 0.11,
    ///              "temperature": 0.11
    ///          }
    ///      ],
    ///      "metadata": {
    ///          "status_code": 200,
    ///          "message": "Succesfully read 2 records, from 4/15/2024 10:57:40 AM, to 4/16/2024 10:57:47 AM"
    ///      }
    /// }
    /// ```
    /// </summary>
    public override async Task<ReadMeasurementsOverTimeSpanResponse> ReadMeasurementsOverTimeSpan(ReadMeasurementsOverTimeSpanRequest request, ServerCallContext context)
    {
        try
        {
            if (request.StartTime == null)
                throw new ArgumentException("Start time must be provided!");

            // Flux query can be injected with malicious strings - its "SQL" injection prone.
            string stopTime = request.StopTime != null ? $", stop: time(v: {request.StopTime.ToDateTime().ToTimestamp()})\n" : string.Empty;
            string pagination = request.PaginationInfo != null && request.PaginationInfo.PageSize > 0 && request.PaginationInfo.Offset >= 0 ?
                $"\t|> limit(n: {request.PaginationInfo.PageSize}, offset: {request.PaginationInfo.Offset})\n" : string.Empty;

            var query = string.Join("",
                $"from(bucket: \"{_influxDBService.DefaultBucket}\")\n",
                $"\t|> range(start: time(v: {request.StartTime.ToDateTime().ToTimestamp()}){stopTime})\n",
                $"\t|> pivot(rowKey: [\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")\n",
                pagination);

            var fluxTables = await _influxDBService.QueryAsync(queryApi => queryApi.QueryAsync(query));
            var records = new List<EnvironmentalSensorTelemetryData>();

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

    /// <summary>
    /// 
    /// - Example query:
    /// 
    /// ```flux
    ///  from(bucket: "EnvironmentalSensorTelemetry")
    ///      |> range(start: time(v: "2024-04-15T10:54:22.730Z"), stop: time(v: "2024-04-16T10:54:31.314Z"))
    ///      |> filter(fn: (r) => r._field == "carbon_oxide")
    ///      |> limit(n: 3, offset: 1)
    /// ```
    /// - Example response:
    /// 
    /// ```json
    /// {
    ///     "records": [
    ///         {
    ///             "field_name": "carbon_oxide",
    ///             "timestamp": "2024-04-16T07:26:25.828Z",
    ///             "value": 0.1
    ///         },
    ///         {
    ///             "field_name": "carbon_oxide",
    ///             "timestamp": "2024-04-16T07:26:56.625Z",
    ///             "value": 0.11
    ///         },
    ///         {
    ///             "field_name": "carbon_oxide",
    ///             "timestamp": "2024-04-16T07:27:28.425Z",
    ///             "value": 0.12
    ///         }
    ///     ],
    ///     "metadata": {
    ///         "status_code": 200,
    ///         "message": "Succesfully read 3 records, from 4/15/2024 10:54:22 AM, to 4/16/2024 10:54:31 AM"
    ///     }
    /// }
    /// ```
    /// </summary>
    public override async Task<ReadFieldsOverTimeSpanResponse> ReadFieldsOverTimeSpan(ReadFieldsOverTimeSpanRequest request, ServerCallContext context)
    {
        try
        {
            if (request.StartTime == null)
                throw new ArgumentException("Start time must be provided!");

            // Flux query can be injected with malicious strings - its "SQL" injection prone.
            string stopTime = request.StopTime != null ? $", stop: time(v: {request.StopTime.ToDateTime().ToTimestamp()})\n" : string.Empty;
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

    /// <summary>
    /// - Example query executed:
    /// ```flux
    /// from(bucket: "EnvironmentalSensorTelemetry")
    ///     |> range(start: time(v: "2020-07-12T02:01:34.385975Z"), stop: time(v: "2020-07-20T02:03:37.264313Z"))
    ///     |> filter(fn: (r) => r._field == "carbon_oxide")
    ///     |> window(every: 1d)
    ///     |> mean()
    ///     |> group(columns: ["device"])
    /// ```
    /// - Raw request payload:
    /// {
    ///     "startTime": "2020-07-12T02:01:34.385Z",
    ///     "stopTime": "2020-07-20T02:03:37.264Z",
    ///     "fieldName": "carbon_oxide",
    ///     "windowDuration": "86400.000000000s",
    ///     "aggregationType": "MEAN"
    /// }
    /// - Example response:
    /// 
    /// ```json
    /// {
    //      "aggregate_windows": [
    //          {
    //              "start": "2024-04-16T07:00:00Z",
    //              "stop": "2024-04-16T08:00:00Z",
    //              "result": 0.105
    //          }
    //      ],
    //      "metadata": {
    //          "status_code": 200,
    //          "message": "Succesfully aggregated Mean values over 1 windows!"
    //      }
    //  }
    /// ``` 
    /// </summary>
    public override async Task<AggregateOverTimeSpanResponse> AggregateOverTimeSpan(AggregateOverTimeSpanRequest request, ServerCallContext context)
    {
        try
        {
            if (request.StartTime == null)
                throw new ArgumentException("Start time must be provided!");

            if (request.StopTime == null)
                throw new ArgumentException("Stop time must be provided!");

            if (string.IsNullOrEmpty(request.FieldName))
                throw new ArgumentException("Field name must be provided!");

            if (request.AggregationType == AggregationType.Nil)
                throw new ArgumentException($"Aggregation type must be one of the following: {string.Join(", ", System.Enum.GetNames<AggregationType>()).Replace("Nil, ", string.Empty)}");

            var query = string.Join(string.Empty,
                $"from(bucket: \"{_influxDBService.DefaultBucket}\")\n",
                $"\t|> range(start: time(v: {request.StartTime.ToDateTime().ToTimestamp()}), stop: time(v: {request.StopTime.ToDateTime().ToTimestamp()}))\n",
                $"\t|> filter(fn: (r) => r._field == \"{request.FieldName}\")\n",
                $"\t|> window(every: {request.WindowDuration.Seconds}s)\n",
                $"\t|> {request.AggregationType.ToString().ToLower()}()\n",
                $"\t|> group(columns: [\"device\"])\n"
            );

            var aggregateResult = await _influxDBService.QueryAsync(async queryApi => await queryApi.QueryAsync(query));

            var aggregateWindows = aggregateResult.SelectMany(fluxTable => fluxTable.Records.Select(record => new AggregateOverTimeSpanResponse.Types.AggregateWindow
            {
                Start = Timestamp.FromDateTime(record.GetStart()!.Value.ToDateTimeUtc()),
                Stop = Timestamp.FromDateTime(record.GetStop()!.Value.ToDateTimeUtc()),
                Value = (double)record.GetValue(),
                Device = (string)record.GetValueByKey("device"),
            })).ToList();

            return new AggregateOverTimeSpanResponse
            {
                AggregateWindows = { aggregateWindows },
                Metadata = new ResponseMetadata
                {
                    Message = $"Succesfully aggregated {System.Enum.GetName(request.AggregationType)} values over {aggregateWindows.Count} windows!",
                    StatusCode = (int)HttpStatusCode.OK,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
            return new AggregateOverTimeSpanResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };
        }
    }

    public override async Task<DeleteForTimeSpanResponse> DeleteForTimeSpan(DeleteForTimeSpanRequest request, ServerCallContext context)
    {
        try
        {
            await _influxDBService.DeleteAsync(async deleteApi => await deleteApi.Delete(
                request.StartTime.ToDateTime(),
                request.StopTime.ToDateTime(),
                $"device=\"{request.Device}\"",
                _influxDBService.DefaultBucket,
                _influxDBService.DefaultOrganization));

            return new DeleteForTimeSpanResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Successful deletion.",
                    StatusCode = (int)HttpStatusCode.OK,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while executing method '{context.Method}' at {DateTime.UtcNow}:\n{ex.StackTrace}");
            return new DeleteForTimeSpanResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };
        }
    }
}

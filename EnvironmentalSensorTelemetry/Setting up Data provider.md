# Setting up Data provider service

- Data provider service is a simple gRPC server, written in .NET Core.
- It provides a simple API for accessing the data from the InfluxDB2 database (set up in previous chapter).
- The said API can be seen bellow (messages are left out for clarity purposes):

```proto
service EnvironmentalSensorTelemetry {
  rpc Ping (google.protobuf.Empty) returns (PingResponse);

  rpc WriteMeasurementsBatched(WriteBatchRequest) returns (WriteDataResponse);
  
  rpc WriteMeasurementsStream(stream EnvironmentalSensorTelemetryData) returns (stream WriteDataResponse);

  rpc ReadMeasurementsOverTimeSpan(ReadMeasurementsOverTimeSpanRequest) returns (ReadMeasurementsOverTimeSpanResponse);
  
  rpc ReadFieldsOverTimeSpan(ReadFieldsOverTimeSpanRequest) returns (ReadFieldsOverTimeSpanResponse);

  rpc AggregateOverTimeSpan(AggregateOverTimeSpanRequest) returns (AggregateOverTimeSpanResponse);

  rpc DeleteForTimeSpan(DeleteForTimeSpanRequest) returns (DeleteForTimeSpanResponse);
}
```

Whole proto file can be seen [here](./Protos/environmental_sensor_telemetry_service.proto). 

- Main components are:
  - [InfluxDBApiFactoryService](./InfluxDB/InfluxDBApiFactoryService.cs) - handles InfluxDB client objects, and provides factory for various InfluxDB client APIs.
  - [EnvironmentalSensorTelemetryService](./GrpcServices/EnvironmentalSensorTelemetryService.cs) - implements the actual gRPC endpoints.

After defining gRPC interface using it's IDL (.proto spec), all defined methods were implemented and tested using [gRPC UI](https://github.com/fullstorydev/grpcui) Web client.

Next step was containerization, i.e. writting the [Dockerfile](./Dockerfile), and integration with existing services (just the InfluxDB service actually) using the Docker compose (see [docker-compose.yml](../docker-compose.yml)).

gRPC UI container was added as a kind of API specification sidecar. [This](https://hub.docker.com/r/wongnai/grpcui) image was used for that purpose.

[Go back to main README.md](../README.md).

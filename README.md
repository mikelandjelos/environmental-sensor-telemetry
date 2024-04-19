# environmental-sensor-telemetry

Repository used for submitting the first project within the subject Internet of Things & Services (Faculty of Electronic Engineering, University of Nis).
The dataset used for this project can be found on [this](https://www.kaggle.com/datasets/garystafford/environmental-sensor-data-132k?resource=download) link.

> ⚠️ Instructions to readers about starting the application
>
> 1. Clone this repository using `git clone https://github.com/mikelandjelos/environmental-sensor-telemetry.git`.
> 2. After cloning, using the command line position yourself inside the repository, where the `docker-compose.yml` is.
> 3. Run the `docker compose up -d` command (you must have `dockerd` running, and have `docker` CLI tool installed).
> 4. That's all:
>       1. InfluxDB Web client can be found on http://localhost:8086 - user `mihajlo`, pass `iotsiotsiots`;
>       2. Web client for testing the gRPC service can be found on on http://localhost:8081.
>       3. Web client for testing the REST service can be found on http://localhost:8084 (TBA);
> 5. For cleanup (stoping and removing all containers), simply run the `docker compose down` command.

More about the actual implementation and process of setting everything up can be found on the pages listed below:

1. [Setting up InfluxDB](./influx/Setting%20up%20InfluxDB.md) - read about setting up InfluxDB within a Docker container.
2. [Setting up Data provider - gRPC server](./EnvironmentalSensorTelemetry/Setting%20up%20Data%20provider.md) - read about setting up data provider gRPC server written in .NET Core, with a gRPCui sidecar.
3. Setting up Rest wrapper - TBA.

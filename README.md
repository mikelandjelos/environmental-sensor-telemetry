# environmental-sensor-telemetry

Repository used for submitting the first project within the subject Internet of Things & Services (Faculty of Electronic Engineering, University of Nis).
The dataset used for this project can be found on [this](https://www.kaggle.com/datasets/garystafford/environmental-sensor-data-132k?resource=download) link.

> ⚠️ Instructions to readers about starting the application
>
> Clone this repository using `git clone https://github.com/mikelandjelos/environmental-sensor-telemetry.git`.
> After cloning, using the command line position yourself inside the repository, where the `docker-compose.yml` is.
> Run the `docker compose up -d` command (you must have `dockerd` running, and have `docker` CLI tool installed).
> That's all, the Web client for testing the REST service on http://localhost:\<rest-port> and the gRPC UI Web client should be available on http://localhost:8081.

More about the actual implementation and process of setting everything up can be found on the pages listed below:

1. [Setting up InfluxDB](./influx/Setting%20up%20InfluxDB.md) - read about setting up InfluxDB within a Docker container.
2. [Setting up Data provider - gRPC server](./EnvironmentalSensorTelemetry/Setting%20up%20Data%20provider.md) - read about setting up data provider gRPC server written in .NET Core, with a gRPCui sidecar.

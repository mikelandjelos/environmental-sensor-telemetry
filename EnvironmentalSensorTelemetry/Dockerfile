FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /EnvironmentalSensorTelemetry

# Copy everything
COPY . ./
RUN dotnet --info
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /EnvironmentalSensorTelemetry
COPY --from=build-env /EnvironmentalSensorTelemetry/out .
ENTRYPOINT ["dotnet", "EnvironmentalSensorTelemetry.dll"]
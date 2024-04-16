using InfluxDB.Client.Core;

namespace app.Models;

[Measurement("sensor_data")]
public class SensorData
{
    [Column("timestamp", IsTimestamp = true)] public DateTime Timestamp { get; init; }
    [Column("device", IsTag = true)] public string? Device { get; init; }
    [Column("carbon_oxide")] public double CarbonOxide { get; init; }
    [Column("humidity")] public double Humidity { get; init; }
    [Column("light")] public bool Light { get; init; }
    [Column("liquid_petroleum_gas")] public double LiquidPetroleumGas { get; init; }
    [Column("motion")] public bool Motion { get; init; }
    [Column("smoke")] public double Smoke { get; init; }
    [Column("temperature")] public double Temperature { get; init; }
}
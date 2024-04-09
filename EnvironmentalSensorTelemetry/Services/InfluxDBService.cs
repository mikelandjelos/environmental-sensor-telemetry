using System.Configuration;
using InfluxDB.Client;
using Tommy;

namespace app.Services;
public class InfluxDBService
{
    private readonly string _token;

    public InfluxDBService(IConfiguration configuration)
    {
        var configFilePath = configuration.GetValue<string>("InfluxDB:ConfigFilePath");

        if (string.IsNullOrEmpty(configFilePath))
            throw new ConfigurationErrorsException("InfluxDB:ConfigFilePath missing from appsettings.json");

        using (StreamReader reader = File.OpenText(configFilePath))
        {
            TomlTable influxConfigs = TOML.Parse(reader);
            _token = influxConfigs["default"]["token"].AsString;
        }

        if (string.IsNullOrEmpty(_token))
            throw new ConfigurationErrorsException("default:token missing or empty in the influx-configs");
    }

    public async Task<bool> Ping()
    {
        using var client = new InfluxDBClient("http://localhost:8086", _token);
        return await client.PingAsync();
    }

    public void Write(Action<WriteApi> action)
    {
        using var client = new InfluxDBClient("http://localhost:8086", _token);
        using var write = client.GetWriteApi();
        action(write);
    }

    public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
    {
        using var client = new InfluxDBClient("http://localhost:8086", _token);
        var query = client.GetQueryApi();
        return await action(query);
    }
}
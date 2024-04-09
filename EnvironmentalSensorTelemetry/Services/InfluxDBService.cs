using System.Configuration;
using InfluxDB.Client;
using Tommy;

namespace app.Services;

/// <summary>
/// Thread-safe InfluxDB client - https://github.com/influxdata/influxdb-client-csharp.
/// </summary>
public class InfluxDBService
{

    private readonly string _url;
    private readonly string _token;

    private readonly string _defaultOrganization;

    private readonly string _defaultBucket;

    public InfluxDBService(IConfiguration configuration)
    {
        // Loading configured token and url from configuration files.
        var configFilePath = configuration.GetValue<string>("InfluxDB:ConfigFilePath");

        if (string.IsNullOrEmpty(configFilePath))
            throw new ConfigurationErrorsException("InfluxDB:ConfigFilePath missing from appsettings.json");

        using (StreamReader reader = File.OpenText(configFilePath))
        {
            TomlTable influxConfigs = TOML.Parse(reader);
            _token = influxConfigs["default"]["token"].AsString;
            _url = influxConfigs["default"]["url"].AsString;
        }

        if (string.IsNullOrEmpty(_token))
            throw new ConfigurationErrorsException("default:token missing or empty in the influx-configs");

        // Loading default organization and bucket from configuration files.

        _defaultOrganization = configuration.GetValue<string>("InfluxDB:DefaultOrganization")!;

        if (string.IsNullOrEmpty(_defaultOrganization))
            throw new ConfigurationErrorsException("InfluxDB:DefaultOrganization missing from appsettings.json");

        _defaultBucket = configuration.GetValue<string>("InfluxDB:DefaultBucket")!;

        if (string.IsNullOrEmpty(_defaultBucket))
            throw new ConfigurationErrorsException("InfluxDB:DefaultBucket missing from appsettings.json");
    }

    public async Task<bool> Ping()
    {
        using var client = new InfluxDBClient(new InfluxDBClientOptions(_url)
        {
            Org = _defaultOrganization,
            Token = _token,
            Bucket = _defaultBucket,
        });
        return await client.PingAsync();
    }

    public void Write(Action<WriteApi> action)
    {
        using var client = new InfluxDBClient(new InfluxDBClientOptions(_url)
        {
            Org = _defaultOrganization,
            Token = _token,
            Bucket = _defaultBucket,
        });
        using var write = client.GetWriteApi();
        action(write);
    }

    public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
    {
        using var client = new InfluxDBClient(new InfluxDBClientOptions(_url)
        {
            Org = _defaultOrganization,
            Token = _token,
            Bucket = _defaultBucket,
        });
        var query = client.GetQueryApi();
        return await action(query);
    }
}
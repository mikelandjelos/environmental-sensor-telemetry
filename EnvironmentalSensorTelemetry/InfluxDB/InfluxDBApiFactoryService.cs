using System.Configuration;
using InfluxDB.Client;

namespace app.Services;

/// <summary>
/// Thread-safe InfluxDB client - https://github.com/influxdata/influxdb-client-csharp.
/// </summary>
public class InfluxDBApiFactoryService
{

    private readonly ILogger<InfluxDBApiFactoryService> _logger;

    private readonly string? _url;
    private readonly string? _token;

    private readonly string _defaultOrganization;

    private readonly string _defaultBucket;

    public string DefaultOrganization => _defaultOrganization;

    public string DefaultBucket => _defaultBucket;

    public InfluxDBApiFactoryService(IConfiguration configuration, ILogger<InfluxDBApiFactoryService> logger)
    {
        _logger = logger;

        var token_secret_file = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN_FILE");

        if (string.IsNullOrEmpty(token_secret_file))
            throw new ConfigurationErrorsException("INFLUXDB_TOKEN_FILE environment variable not set!");

        _token = File.ReadAllText(token_secret_file);

        _logger.LogCritical(_token);
        
        _url = Environment.GetEnvironmentVariable("INFLUXDB_URL");
        _logger.LogCritical(_url);

        if (string.IsNullOrEmpty(_url))
            throw new ConfigurationErrorsException("INFLUX_URL environment variable not set!");

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
        using var writeApi = client.GetWriteApi();
        action(writeApi);
    }

    public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
    {
        using var client = new InfluxDBClient(new InfluxDBClientOptions(_url)
        {
            Org = _defaultOrganization,
            Token = _token,
            Bucket = _defaultBucket,
        });
        var queryApi = client.GetQueryApi();
        return await action(queryApi);
    }

    public async Task DeleteAsync(Func<DeleteApi, Task> action)
    {
        using var client = new InfluxDBClient(new InfluxDBClientOptions(_url)
        {
            Org = _defaultOrganization,
            Token = _token,
            Bucket = _defaultBucket,
        });
        var deleteApi = client.GetDeleteApi();
        await action(deleteApi);
    }
}
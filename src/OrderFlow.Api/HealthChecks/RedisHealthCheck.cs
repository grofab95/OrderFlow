using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace OrderFlow.Api.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();

            await database.PingAsync().WaitAsync(cancellationToken);

            return HealthCheckResult.Healthy("Redis responded to PING.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("Redis did not respond to PING.");
        }
    }
}

namespace OrderFlow.Api.HealthChecks;

public static class HealthCheckNames
{
    public const string Database = "postgres";
    public const string Redis = "redis";
    public const string MassTransit = "masstransit-bus";
}

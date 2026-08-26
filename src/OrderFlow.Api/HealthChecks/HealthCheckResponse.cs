namespace OrderFlow.Api.HealthChecks;

public record HealthCheckResponse(
    string Status,
    double DurationMs,
    IReadOnlyCollection<HealthCheckEntryResponse> Checks);

public record HealthCheckEntryResponse(
    string Name,
    string Status,
    double DurationMs);

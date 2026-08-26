using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using OrderFlow.Api.HealthChecks;

namespace OrderFlow.Api.Tests.HealthChecks;

public class HealthCheckEndpointsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task LiveEndpointShouldReturnOkWhenApplicationIsRunning()
    {
        // Arrange
        using var host = await HealthCheckTestHost.Start();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await ReadPayload(response);
        Assert.Equal(HealthStatus.Healthy.ToString(), payload.Status);
        Assert.Empty(payload.Checks);
    }

    [Fact]
    public async Task LiveEndpointShouldIgnoreDependencyChecksWhenDependencyIsUnhealthy()
    {
        // Arrange
        var dependency = FakeHealthCheck.Returning(HealthStatus.Unhealthy);
        using var host = await HealthCheckTestHost.Start((HealthCheckNames.Database, dependency));
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await ReadPayload(response);
        Assert.Equal(HealthStatus.Healthy.ToString(), payload.Status);
        Assert.Empty(payload.Checks);
        await dependency.DidNotReceive().CheckHealthAsync(
            Arg.Any<HealthCheckContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadyEndpointShouldReturnOkWhenAllDependenciesAreHealthy()
    {
        // Arrange
        using var host = await HealthCheckTestHost.Start(
            (HealthCheckNames.Database, FakeHealthCheck.Returning(HealthStatus.Healthy)),
            (HealthCheckNames.Redis, FakeHealthCheck.Returning(HealthStatus.Healthy)),
            (HealthCheckNames.MassTransit, FakeHealthCheck.Returning(HealthStatus.Healthy)));
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await ReadPayload(response);
        Assert.Equal(HealthStatus.Healthy.ToString(), payload.Status);
        Assert.Equal(3, payload.Checks.Count);
        Assert.All(payload.Checks, check =>
            Assert.Equal(HealthStatus.Healthy.ToString(), check.Status));
        Assert.Contains(payload.Checks, check => check.Name == HealthCheckNames.Database);
        Assert.Contains(payload.Checks, check => check.Name == HealthCheckNames.Redis);
        Assert.Contains(payload.Checks, check => check.Name == HealthCheckNames.MassTransit);
    }

    [Fact]
    public async Task ReadyEndpointShouldReturnServiceUnavailableWhenDependencyIsUnhealthy()
    {
        // Arrange
        using var host = await HealthCheckTestHost.Start(
            (HealthCheckNames.Database, FakeHealthCheck.Returning(HealthStatus.Healthy)),
            (HealthCheckNames.Redis, FakeHealthCheck.Returning(HealthStatus.Unhealthy)));
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var payload = await ReadPayload(response);
        Assert.Equal(HealthStatus.Unhealthy.ToString(), payload.Status);
        Assert.Contains(payload.Checks, check =>
            check.Name == HealthCheckNames.Redis &&
            check.Status == HealthStatus.Unhealthy.ToString());
    }

    [Fact]
    public async Task ReadyEndpointShouldReturnServiceUnavailableWhenDependencyIsDegraded()
    {
        // Arrange
        using var host = await HealthCheckTestHost.Start(
            (HealthCheckNames.Database, FakeHealthCheck.Returning(HealthStatus.Healthy)),
            (HealthCheckNames.MassTransit, FakeHealthCheck.Returning(HealthStatus.Degraded)));
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var payload = await ReadPayload(response);
        Assert.Equal(HealthStatus.Degraded.ToString(), payload.Status);
        Assert.Contains(payload.Checks, check =>
            check.Name == HealthCheckNames.MassTransit &&
            check.Status == HealthStatus.Degraded.ToString());
    }

    [Fact]
    public async Task ReadyEndpointShouldNotExposeExceptionDetailsWhenDependencyThrows()
    {
        // Arrange
        var dependency = Substitute.For<IHealthCheck>();
        dependency
            .CheckHealthAsync(Arg.Any<HealthCheckContext>(), Arg.Any<CancellationToken>())
            .Returns<Task<HealthCheckResult>>(_ =>
                throw new InvalidOperationException("Host=localhost;Password=postgres"));
        using var host = await HealthCheckTestHost.Start((HealthCheckNames.Database, dependency));
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at OrderFlow", body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HealthCheckResponse> ReadPayload(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<HealthCheckResponse>(body, SerializerOptions)!;
    }
}

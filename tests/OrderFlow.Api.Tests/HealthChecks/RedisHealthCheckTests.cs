using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Api.HealthChecks;
using StackExchange.Redis;

namespace OrderFlow.Api.Tests.HealthChecks;

public class RedisHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsyncShouldReturnHealthyWhenPingSucceeds()
    {
        // Arrange
        var database = Substitute.For<IDatabase>();
        database.PingAsync(Arg.Any<CommandFlags>()).Returns(TimeSpan.FromMilliseconds(1));
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        connectionMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(database);
        var healthCheck = new RedisHealthCheck(connectionMultiplexer);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        await database.Received(1).PingAsync(Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task CheckHealthAsyncShouldReturnUnhealthyWhenPingFails()
    {
        // Arrange
        var database = Substitute.For<IDatabase>();
        database.PingAsync(Arg.Any<CommandFlags>())
            .ThrowsAsync(new InvalidOperationException("Host=localhost:6379;Password=secret"));
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        connectionMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(database);
        var healthCheck = new RedisHealthCheck(connectionMultiplexer);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Null(result.Exception);
        Assert.DoesNotContain("Password", result.Description ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static HealthCheckContext CreateContext()
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                HealthCheckNames.Redis,
                Substitute.For<IHealthCheck>(),
                HealthStatus.Unhealthy,
                [HealthCheckTags.Ready])
        };
    }
}

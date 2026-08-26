using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using OrderFlow.Api.HealthChecks;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Tests.HealthChecks;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsyncShouldReturnHealthyWhenDatabaseCanConnect()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var healthCheck = new DatabaseHealthCheck(dbContext);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsyncShouldReturnUnhealthyWhenDatabaseIsUnreachable()
    {
        // Arrange
        var dbContext = CreateDbContext();
        await dbContext.DisposeAsync();
        var healthCheck = new DatabaseHealthCheck(dbContext);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Null(result.Exception);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static HealthCheckContext CreateContext()
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                HealthCheckNames.Database,
                Substitute.For<IHealthCheck>(),
                HealthStatus.Unhealthy,
                [HealthCheckTags.Ready])
        };
    }
}

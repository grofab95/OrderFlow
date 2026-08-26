using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OrderFlow.Api.Extensions;
using OrderFlow.Api.HealthChecks;

namespace OrderFlow.Api.Tests.HealthChecks;

public class HealthCheckRegistrationTests
{
    [Fact]
    public async Task AddApplicationHealthChecksShouldTagDependencyChecksAsReadyWhenRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDatabase(CreateConfiguration());

        // Act
        services.AddApplicationHealthChecks();

        // Assert
        var registrations = await GetRegistrations(services);
        Assert.Contains(registrations, registration =>
            registration.Name == HealthCheckNames.Database &&
            registration.Tags.Contains(HealthCheckTags.Ready));
        Assert.Contains(registrations, registration =>
            registration.Name == HealthCheckNames.Redis &&
            registration.Tags.Contains(HealthCheckTags.Ready));
    }

    [Fact]
    public async Task AddApplicationHealthChecksShouldTagMassTransitCheckAsReadyWhenBusIsRegistered()
    {
        // Arrange
        var services = CreateServicesWithBus();

        // Act
        services.AddApplicationHealthChecks();

        // Assert
        var registrations = await GetRegistrations(services);
        Assert.Contains(registrations, registration =>
            registration.Name == HealthCheckNames.MassTransit &&
            registration.Tags.Contains(HealthCheckTags.Ready));
    }

    [Fact]
    public async Task AddApplicationHealthChecksShouldBoundTimeoutWhenCheckIsTaggedAsReady()
    {
        // Arrange
        var services = CreateServicesWithBus();
        services.AddDatabase(CreateConfiguration());

        // Act
        services.AddApplicationHealthChecks();

        // Assert
        var registrations = (await GetRegistrations(services))
            .Where(registration => registration.Tags.Contains(HealthCheckTags.Ready))
            .ToArray();
        Assert.NotEmpty(registrations);
        Assert.All(registrations, registration =>
        {
            Assert.NotEqual(Timeout.InfiniteTimeSpan, registration.Timeout);
            Assert.True(registration.Timeout > TimeSpan.Zero);
        });
    }

    private static ServiceCollection CreateServicesWithBus()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMassTransit(x =>
        {
            x.ConfigureHealthCheckOptions(options =>
            {
                options.Name = HealthCheckNames.MassTransit;
                options.Tags.Add(HealthCheckTags.Ready);
            });

            x.UsingInMemory();
        });

        return services;
    }

    private static async Task<IReadOnlyCollection<HealthCheckRegistration>> GetRegistrations(
        IServiceCollection services)
    {
        await using var provider = services.BuildServiceProvider();

        return provider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations
            .ToArray();
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Database=orderflow;Username=postgres;Password=postgres"
            })
            .Build();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Api.Features.Inventory;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.HealthChecks;
using OrderFlow.Api.Persistence;
using StackExchange.Redis;

namespace OrderFlow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly TimeSpan DependencyCheckTimeout = TimeSpan.FromSeconds(3);

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });
    }
    
    public static void AddOrderService(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
    }
    
    public static void AddInventoryService(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
    }

    public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis")!);
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddStackExchangeRedisCache(_ => { });

        services
            .AddOptions<RedisCacheOptions>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.InstanceName = "MyApp:";
                options.ConnectionMultiplexerFactory = () =>
                    Task.FromResult(serviceProvider.GetRequiredService<IConnectionMultiplexer>());
            });
    }

    public static void AddApplicationHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                HealthCheckNames.Database,
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Ready],
                timeout: DependencyCheckTimeout)
            .AddCheck<RedisHealthCheck>(
                HealthCheckNames.Redis,
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Ready],
                timeout: DependencyCheckTimeout);

        services.PostConfigure<HealthCheckServiceOptions>(options =>
        {
            foreach (var registration in options.Registrations)
            {
                if (registration.Tags.Contains(HealthCheckTags.Ready) &&
                    registration.Timeout == Timeout.InfiniteTimeSpan)
                {
                    registration.Timeout = DependencyCheckTimeout;
                }
            }
        });
    }
}

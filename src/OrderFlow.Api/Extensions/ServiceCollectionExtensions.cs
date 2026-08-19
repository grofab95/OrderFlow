using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Features.Inventory;
using OrderFlow.Api.Features.Orders;
using OrderFlow.Api.Persistence;
using StackExchange.Redis;

namespace OrderFlow.Api.Extensions;

public static class ServiceCollectionExtensions
{
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
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "MyApp:";
        });
    }
}
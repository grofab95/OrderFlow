using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Domain.Inventories;
using OrderFlow.Api.Domain.Orders;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });
    }
    
    public static void AddOrderService(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
    }
}
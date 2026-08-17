using OrderFlow.Api.Domain.Orders;
using OrderFlow.Api.Persistence;

namespace OrderFlow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            //options.UseSqlServer("YourConnectionString");
        });
    }
    
    public static void AddOrderService(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
    }
}
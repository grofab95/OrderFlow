using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Domain.Orders;
using OrderFlow.Api.Domain.Prpducts;

namespace OrderFlow.Api.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
}
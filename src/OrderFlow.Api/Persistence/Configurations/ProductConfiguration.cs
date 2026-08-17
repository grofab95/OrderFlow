using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Api.Domain.Products;

namespace OrderFlow.Api.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();
        
        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.HasData(
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Laptop",
                Quantity = 10,
                Price = 4999.99m
            },
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Monitor",
                Quantity = 15,
                Price = 1299.99m
            },
            new
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Keyboard",
                Quantity = 30,
                Price = 249.99m
            },
            new
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Mouse",
                Quantity = 40,
                Price = 149.99m
            },
            new
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Headphones",
                Quantity = 20,
                Price = 399.99m
            },
            new
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Webcam",
                Quantity = 12,
                Price = 329.99m
            },
            new
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "USB-C Hub",
                Quantity = 25,
                Price = 199.99m
            },
            new
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "SSD 1TB",
                Quantity = 18,
                Price = 349.99m
            },
            new
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Docking Station",
                Quantity = 8,
                Price = 699.99m
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Microphone",
                Quantity = 14,
                Price = 449.99m
            });
    }
}
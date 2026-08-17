using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderFlow.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price", "Quantity" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Laptop", 4999.99m, 10 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Monitor", 1299.99m, 15 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Keyboard", 249.99m, 30 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Mouse", 149.99m, 40 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Headphones", 399.99m, 20 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Webcam", 329.99m, 12 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "USB-C Hub", 199.99m, 25 },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "SSD 1TB", 349.99m, 18 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Docking Station", 699.99m, 8 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Microphone", 449.99m, 14 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        }
    }
}

using MassTransit;
using OrderFlow.Api.ErrorHandling;
using OrderFlow.Api.Extensions;
using OrderFlow.Api.Features.Inventory.Consumers;
using OrderFlow.Api.Features.Orders.Consumers;
using OrderFlow.Api.Features.Payments.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddOrderService();
builder.Services.AddInventoryService();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 5672, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
    });
    
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<InventoryReservedConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OrderFlow API v1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseExceptionHandler();
app.Run();
using System.Text.Json.Serialization;
using MassTransit;
using OrderFlow.Api.ErrorHandling;
using OrderFlow.Api.Extensions;
using OrderFlow.Api.Features.Inventory.Consumers;
using OrderFlow.Api.Features.Orders.Consumers;
using OrderFlow.Api.Features.Payments.Consumers;
using OrderFlow.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddOrderService();
builder.Services.AddInventoryService();
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<InventoryReservedConsumer>();
    x.AddConsumer<InventoryReservationFailedConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();

    x.AddEntityFrameworkOutbox<AppDbContext>(outbox =>
    {
        outbox.UsePostgres();
        outbox.UseBusOutbox();
    });

    x.AddConfigureEndpointsCallback((context, _, endpoint) =>
    {
        endpoint.UseMessageRetry(retry =>
            retry.Interval(3, TimeSpan.FromSeconds(2)));
        
        endpoint.UseEntityFrameworkOutbox<AppDbContext>(context);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 5672, "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
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
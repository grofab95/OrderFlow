using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.Options;
using NLog.Web;
using OrderFlow.Api.Configuration;
using OrderFlow.Api.ErrorHandling;
using OrderFlow.Api.Extensions;
using OrderFlow.Api.Features.Inventory.Consumers;
using OrderFlow.Api.Features.Orders.Consumers;
using OrderFlow.Api.Features.Payments.Consumers;
using OrderFlow.Api.HealthChecks;
using OrderFlow.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

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
builder.Services.AddApplicationHealthChecks();

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionKey))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<InventoryReservedConsumer>();
    x.AddConsumer<InventoryReservationFailedConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();

    x.ConfigureHealthCheckOptions(options =>
    {
        options.Name = HealthCheckNames.MassTransit;
        options.Tags.Add(HealthCheckTags.Ready);
    });

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
        var options = context
            .GetRequiredService<IOptions<RabbitMqOptions>>()
            .Value;
        
        cfg.Host(options.Host, (ushort)options.Port, "/", h =>
        {
            h.Username(options.Username);
            h.Password(options.Password);
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
app.MapHealthCheckEndpoints();
app.UseExceptionHandler();
app.Run();
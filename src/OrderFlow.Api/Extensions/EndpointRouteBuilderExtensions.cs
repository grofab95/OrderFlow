using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Api.HealthChecks;

namespace OrderFlow.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void MapHealthCheckEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/health/live",
                (
                    HttpContext context,
                    [FromServices] HealthCheckService healthChecks
                ) => WriteResponse(
                    context,
                    healthChecks,
                    static _ => false))
            .WithName("HealthLive")
            .WithTags("Health")
            .Produces(
                StatusCodes.Status200OK,
                contentType: "application/json");

        endpoints
            .MapGet(
                "/health/ready",
                (
                    HttpContext context,
                    [FromServices] HealthCheckService healthChecks
                ) => WriteResponse(
                    context,
                    healthChecks,
                    static registration =>
                        registration.Tags.Contains(HealthCheckTags.Ready)))
            .WithName("HealthReady")
            .WithTags("Health")
            .Produces(
                StatusCodes.Status200OK,
                contentType: "application/json")
            .Produces(
                StatusCodes.Status503ServiceUnavailable,
                contentType: "application/json");
    }

    private static async Task WriteResponse(
        HttpContext context,
        HealthCheckService healthChecks,
        Func<HealthCheckRegistration, bool> predicate)
    {
        var report = await healthChecks.CheckHealthAsync(
            predicate,
            context.RequestAborted);

        context.Response.StatusCode = report.Status switch
        {
            HealthStatus.Healthy => StatusCodes.Status200OK,
            
            _ => StatusCodes.Status503ServiceUnavailable
        };

        await HealthCheckResponseWriter.Write(context, report);
    }
}
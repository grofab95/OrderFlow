using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OrderFlow.Api.Extensions;
using OrderFlow.Api.HealthChecks;

namespace OrderFlow.Api.Tests.HealthChecks;

internal static class HealthCheckTestHost
{
    public static Task<IHost> Start(params (string Name, IHealthCheck Check)[] dependencyChecks)
    {
        return new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();

                webHost.ConfigureServices(services =>
                {
                    services.AddRouting();

                    var healthChecks = services.AddHealthChecks();

                    foreach (var (name, check) in dependencyChecks)
                    {
                        healthChecks.AddCheck(name, check, tags: [HealthCheckTags.Ready]);
                    }
                });

                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapHealthCheckEndpoints());
                });
            })
            .StartAsync();
    }
}

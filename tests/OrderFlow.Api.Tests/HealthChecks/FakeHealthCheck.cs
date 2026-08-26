using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;

namespace OrderFlow.Api.Tests.HealthChecks;

internal static class FakeHealthCheck
{
    public static IHealthCheck Returning(HealthStatus status)
    {
        var check = Substitute.For<IHealthCheck>();

        check.CheckHealthAsync(Arg.Any<HealthCheckContext>(), Arg.Any<CancellationToken>())
            .Returns(new HealthCheckResult(status));

        return check;
    }
}

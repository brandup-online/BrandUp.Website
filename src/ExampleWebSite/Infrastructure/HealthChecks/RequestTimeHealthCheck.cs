using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ExampleWebSite.Infrastructure.HealthChecks
{
    public class RequestTimeHealthCheck : IHealthCheck
    {
        Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Website is ready."));
        }
    }
}
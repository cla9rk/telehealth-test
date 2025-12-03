using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace org.cchmc.{{cookiecutter.namespace}}.apphost.Configuration
{
    public static partial class ApiBuilder
    {
        // This is a method to initially register the health check setup, but your individual health check registrations belong in the .data project (where the health check itself lives)
        public static void RegisterHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                    .AddProcessAllocatedMemoryHealthCheck(500, "Memory"); // Report unhealthy if app is > 500mb of memory. This health check also reports the numerical memory usage.
        }

        public static void ConfigureHealthChecks(this WebApplication app)
        {
            app.UseCors("healthCheckPolicy");
            app.MapHealthChecks("/_health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes =
                {   // Make everything return 200 in addition to the status (reduces confusion)
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status200OK
                }
            });
        }
    }
}

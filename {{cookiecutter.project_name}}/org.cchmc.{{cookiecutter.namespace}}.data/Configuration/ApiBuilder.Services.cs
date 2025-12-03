using Microsoft.Extensions.DependencyInjection;

namespace org.cchmc.{{cookiecutter.namespace}}.data.Configuration
{
    public static partial class ApiBuilder
    {
        public static void AddDataServices(this IServiceCollection services)
        {
            // register any services here

            // register any Http clients here
            //services.AddHttpClient<IMyServiceType>(c =>
            //{
            //    c.BaseAddress = new Uri("https://localhost");
            //});

            // don't forget to add health checks for these services if necessary
            // if adding a health check for a URI, you need to add nuget package AspNetCore.HealthChecks.Uris
        }
    }
}

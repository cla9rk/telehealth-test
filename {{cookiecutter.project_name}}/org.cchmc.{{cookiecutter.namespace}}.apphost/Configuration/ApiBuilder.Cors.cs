using Microsoft.Extensions.DependencyInjection;

namespace org.cchmc.{{cookiecutter.namespace}}.apphost.Configuration
{
    public static partial class ApiBuilder
    {
        public static void AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
                options.AddPolicy("healthCheckPolicy",
                    corsPolicyBuilder => corsPolicyBuilder
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins("*")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .Build()
                )
            );
        }
    }
}
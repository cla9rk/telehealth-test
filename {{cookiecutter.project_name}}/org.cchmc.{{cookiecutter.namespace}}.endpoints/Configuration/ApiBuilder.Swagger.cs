using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace org.cchmc.{{cookiecutter.namespace}}.endpoints.Configuration
{
    public static partial class ApiBuilder
    {
        public static void AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "{{cookiecutter.project_name}}", Version = "v1" });
            });
        }

        public static void UseSwaggerReference(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "{{cookiecutter.project_name}} v1"); });
        }
    }
}

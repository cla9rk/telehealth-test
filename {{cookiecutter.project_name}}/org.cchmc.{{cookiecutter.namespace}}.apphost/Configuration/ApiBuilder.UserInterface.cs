using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using org.cchmc.{{cookiecutter.namespace}}.models.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace org.cchmc.{{cookiecutter.namespace}}.apphost.Configuration
{
    public static partial class ApiBuilder
    {
        static List<string> apiRoutes = ["/api", "/hubs", "/_"];
        public static void AddUIHealthCheck(this IServiceCollection services)
        {
            var healthOptions = GlobalConfiguration.CustomOptions.HealthCheckConfig;
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var uri = healthOptions.IsLocal ? healthOptions.LocalUIAddress : $"https://{Environment.MachineName}.{domainName}/{{cookiecutter.project_nospace}}";
            services.AddHealthChecks().AddUrlGroup(new Uri(uri), "User Interface");
        }

        public static void SetupUserInterface(this WebApplication app)
        {
            static bool IsApiRequest(HttpContext context) => apiRoutes.Any(route => context.Request.Path.ToString().StartsWith(route));
            app.UseWhen(context => !IsApiRequest(context), appBuilder => appBuilder.Run(async context =>
            {
                if (!IsApiRequest(context))
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.SendFileAsync(Path.Join(app.Environment.WebRootPath, "index.html"));
                }
            }));
        }
    }
}

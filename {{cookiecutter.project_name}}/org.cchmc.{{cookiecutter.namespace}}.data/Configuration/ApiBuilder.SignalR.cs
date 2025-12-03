using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace org.cchmc.{{cookiecutter.namespace}}.data.Configuration
{
    public static partial class ApiBuilder
    {
        public static void AddSignalRHubs(this IServiceCollection services)
        {
            services.AddSignalR();
            // Hubs do not need to be registered here, but stuff like a cache would
        }

        public static void MapSignalRHubs(this WebApplication app)
        {
            app.UseWebSockets();
            //app.MapHub<MyHub>("hubs/myroute");
        }
    }
}

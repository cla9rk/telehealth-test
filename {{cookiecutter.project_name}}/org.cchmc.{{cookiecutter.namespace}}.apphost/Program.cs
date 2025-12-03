using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using org.cchmc.{{cookiecutter.namespace}}.apphost.Configuration;
using org.cchmc.{{cookiecutter.namespace}}.apphost.Middleware;
using org.cchmc.{{cookiecutter.namespace}}.auth.Configuration;
using org.cchmc.{{cookiecutter.namespace}}.data.Configuration;
using org.cchmc.{{cookiecutter.namespace}}.endpoints.Configuration;
using org.cchmc.{{cookiecutter.namespace}}.models.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.apphost
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
                builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: false);

                // If your scopes aren't set up correctly, your build will fail instead of blowing up when you run it
                builder.Host.UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    options.ValidateOnBuild = true;
                });

                GlobalConfiguration.DbConnectionString = builder.Configuration.GetConnectionString("database");
                GlobalConfiguration.CustomOptions = builder.Configuration.GetRequiredSection("customOptions").Get<CustomOptions>();

                builder.RegisterLogging(); // sets up NLog

                builder.Services.AddSignalRHubs(); // remove if not using SignalR
                builder.Services.AddAuthenticationOptions(builder.Configuration); // remove if not using SSO
                builder.Services.AddSwaggerServices();
                builder.Services.AddDbContextAndRepositories();
                builder.Services.AddDataServices();
                builder.Services.AddCorsConfiguration();
                builder.Services.RegisterHealthChecks();
                builder.Services.AddUIHealthCheck();
                builder.Services.PrepareApis(); // adds validation

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwaggerReference();
                }
                else
                {
                    app.UseExceptionHandler();
                    app.UseHsts();
                }

                app.ConfigureHealthChecks();

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseAuthenticationAndAuthorization(); // remove if not using SSO

                app.UseMiddleware<RequestResponseLoggingMiddleware>();

                app.MapSignalRHubs(); // remove if not using SignalR
                app.MapSingleSignOnEndpoints(); // remove if not using SSO
                app.MapSampleEndpoints(); // Replace with your endpoint extensions

                app.SetupUserInterface();
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}

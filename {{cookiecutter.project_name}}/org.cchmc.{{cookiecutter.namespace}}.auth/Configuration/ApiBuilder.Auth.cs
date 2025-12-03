using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces;
using org.cchmc.{{cookiecutter.namespace}}.auth.Services;
using org.cchmc.{{cookiecutter.namespace}}.auth.Settings;
using System;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Configuration
{
    public static partial class ApiBuilder
    {
        // Add `AuthOptions` to DI, register SSO Service, add SSO HTTP client to DI, adds SSO health check
        public static void AddAuthenticationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var authOptions = configuration.GetSection("authenticationoptions").Get<AuthOptions>();

            // These two lines allow me to inject AuthOptions directly where needed instead of IOptions<AuthOptions>
            services.Configure<AuthOptions>(configuration.GetSection("authenticationoptions"));
            services.AddSingleton(registeredServices => registeredServices.GetRequiredService<IOptions<AuthOptions>>().Value);

            services.AddScoped<ISingleSignOn, SingleSignOnService>();
            services.AddHttpClient<ISingleSignOn>(c =>
            {
                c.DefaultRequestHeaders.Add("SSO-API-Key", authOptions.ApplicationEnvironmentId);
                c.BaseAddress = new Uri(authOptions.SingleSignOnEndpoint);
            });
            services.AddHealthChecks().AddUrlGroup(new Uri(authOptions.SingleSignOnRedirectUrl), "Single Sign On");

            // add the context accessor to DI because code will need it for `HttpContext` that was previously available via the Controller
            services.AddHttpContextAccessor();

            // specify authentication is via cookie and establish some options for 401/403
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, // sets up auth as a cookie
                        opt =>
                        {
                            opt.Events.OnRedirectToLogin = ctx =>
                            {
                                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            };
                            opt.Events.OnRedirectToAccessDenied = ctx =>
                            {
                                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return Task.CompletedTask;
                            };
                        });

            // create a policy for endpoints to use
            services.AddAuthorizationBuilder()
                    .AddPolicy("default", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
                    });

            // defines the cookie validation rules
            services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieOptions>(); // sets up the options for the cookie

            // adds handler for refreshing the cookie
            services.AddScoped<IAuthorizationMiddlewareResultHandler, RefreshAuthorizationMiddlewareResultHandler>();
        }

        public static void UseAuthenticationAndAuthorization(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}

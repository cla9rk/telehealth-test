using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using org.cchmc.{{cookiecutter.namespace}}.auth.Endpoints;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Configuration
{
    public static partial class EndpointMapper
    {
        public static void MapSingleSignOnEndpoints(this WebApplication app)
        {
            var ssoItems = app.MapGroup("/api/Auth");

            ssoItems.MapGet("/SignIn", AuthEndpoints.SignIn)
                    .WithName("SignIn")
                    .WithDescription("Gets sign-in information for connecting to SSO");

            ssoItems.MapPost("/CompleteSignIn", AuthEndpoints.CompleteSignIn)
                    .WithName("CompleteSignIn")
                    .WithDescription("Takes a signInToken and exchanges it for a cookie & JWT");

            ssoItems.MapPost("/SsoSignOut", AuthEndpoints.SsoSignOut)
                    .WithName("SsoSignOut")
                    .WithDescription("Destroys the user's cookie and revokes their session in SSO");

            ssoItems.MapPost("/AcknowledgeAlert/{alertId}", AuthEndpoints.AcknowledgeAlert)
                    .WithName("AcknowledgeAlert")
                    .WithDescription("Marks the alert as read")
                    .RequireAuthorization();

            ssoItems.MapGet("/GetUserInformation", AuthEndpoints.GetUserInformation)
                    .WithName("GetUserInformation")
                    .WithDescription("Interrogates the user's cookie to rebuild their JWT and user object")
                    .RequireAuthorization();
        }
    }
}

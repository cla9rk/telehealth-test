using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using org.cchmc.{{cookiecutter.namespace}}.auth.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Endpoints
{
    public class AuthEndpoints()
    {
        public static Ok<SingleSignOnRedirectOptions> SignIn(AuthOptions _authOptions)
        {
            return TypedResults.Ok(new SingleSignOnRedirectOptions()
            {
                SingleSignOnUrl = _authOptions.SingleSignOnRedirectUrl,
                EnvironmentId = _authOptions.ApplicationEnvironmentId
            });
        }

        public static async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult, ProblemHttpResult>> CompleteSignIn([FromForm] string signInToken, [FromServices] ISingleSignOn _ssoService, [FromServices] ILogger<AuthEndpoints> _logger, [FromServices] HttpContext httpContext, [FromServices]AuthOptions _authOptions)
        {
            try
            {
                var newTokens = await _ssoService.CompleteSignIn(signInToken, IpAddressHelper.IpAddress(httpContext));
                if (newTokens != null)
                {
                    var response = new LoginResponse()
                    {
                        User = newTokens.User,
                        Alerts = newTokens.ActiveAlerts
                    };
                    var claims = AuthCookieHelper.GenerateClaims(newTokens, newTokens.RefreshToken, _authOptions.ExpirationMinutes);
                    await AuthCookieHelper.SetCookie(httpContext, claims, newTokens.ExpiresOnUtc);
                    var token = AuthCookieHelper.GenerateJwtFromUserInfo(claims, _authOptions.IssuerSigningKey);
                    httpContext.Response.Headers.Append("sso_user", token);

                    return TypedResults.Ok(response);
                }
                return TypedResults.Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sign in for signInToken {signInToken}.", signInToken.Replace(Environment.NewLine, ""));
                return TypedResults.Problem("An error occurred.");
            }
        }

        public static async Task<Results<Ok, ProblemHttpResult>> SsoSignOut(HttpContext httpContext, [FromServices] ISingleSignOn _ssoService, ILogger<AuthEndpoints> _logger, AuthOptions _authOptions)
        {
            try
            {
                string refreshToken = httpContext.User.Claims.FirstOrDefault(c => c.Type == AuthCookieHelper.RefreshTokenClaimName)?.Value;
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return TypedResults.Ok();

                await _ssoService.SignOut(refreshToken, _authOptions.ApplicationEnvironmentId, IpAddressHelper.IpAddress(httpContext));
                await AuthCookieHelper.SignOut(httpContext);

                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing out.");
                return TypedResults.Problem("An error occurred.");
            }
        }

        public static async Task<Results<Ok<bool>, ProblemHttpResult>> AcknowledgeAlert([FromRoute] Guid alertId, HttpContext httpContext, [FromServices] ISingleSignOn _ssoService, ILogger<AuthEndpoints> _logger)
        {
            try
            {
                string userId = httpContext.User?.Claims?.FirstOrDefault(p => p.Type == JwtRegisteredClaimNames.NameId)?.Value;
                var result = await _ssoService.AcknowledgeAlert(alertId.ToString(), userId ?? Guid.Empty.ToString());
                return TypedResults.Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alert {alertId}.", alertId);
                return TypedResults.Problem("An error occurred.");
            }
        }

        public static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> GetUserInformation(HttpContext httpContext, [FromServices] ISingleSignOn _ssoService, ILogger<AuthEndpoints> _logger, AuthOptions _authOptions)
        {
            try
            {
                AuthResponse result = AuthCookieHelper.GenerateUserFromClaims(httpContext.User.Claims.ToList());

                result.User.ThumbnailPhoto = await _ssoService.GetThumbnail(httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);

                var token = AuthCookieHelper.GenerateJwtFromUserInfo(httpContext.User.Claims.ToArray(), _authOptions.IssuerSigningKey);
                httpContext.Response.Headers.Append("sso_user", token);
                return TypedResults.Ok(new LoginResponse()
                {
                    Alerts = result.ActiveAlerts,
                    User = result.User
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user information.");
                return TypedResults.Problem("An error occurred.");
            }
        }
    }
}

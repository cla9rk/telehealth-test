using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using org.cchmc.{{cookiecutter.namespace}}.auth.Settings;

namespace org.cchmc.{{cookiecutter.namespace}}.auth
{
    // This is not actually middleware, it just handles the result OF the base auth middleware
    // This doesn't handle parallel requests that might create two refresh tokens. However, one is lost to time because this WILL result in just one cookie being set.
    public class RefreshAuthorizationMiddlewareResultHandler(ISingleSignOn _sso, AuthOptions _authOptions, ILogger<RefreshAuthorizationMiddlewareResultHandler> _logger) : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (context.User == null || !context.User.Claims.Any())
            {   // If there's no user at all (presumably no cookie) 401
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                _logger.LogWarning("User is null");
                return;
            }

            Claim expirationClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration);
            if (expirationClaim == null)
            {   // User should have an expiration claim which is the expiration of the auth cookie
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                _logger.LogWarning("User doesn't have an expiration claim (user has {claimCount} claims)", context.User.Claims.Count());
                return;
            }

            var expirationDate = DateTime.Parse(expirationClaim.Value);
            if (expirationDate <= DateTime.Now)
            {
                string refreshToken = context.User.Claims.FirstOrDefault(c => c.Type == AuthCookieHelper.RefreshTokenClaimName)?.Value;
                var success = await RefreshCookie(context, refreshToken);
                if (!success)
                {   // Unable to refresh the token
                    _logger.LogWarning("Unable to refresh token");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        protected async Task<bool> RefreshCookie(HttpContext context, string refreshToken)
        {
            string ipAddress = IpAddressHelper.IpAddress(context);
            AuthResponse authResponse = await _sso.RefreshToken(refreshToken, _authOptions.ApplicationEnvironmentId, ipAddress);

            if (authResponse == null) return false;

            Claim[] claims = AuthCookieHelper.GenerateClaims(authResponse, authResponse.RefreshToken, _authOptions.ExpirationMinutes);

            // Because this cookie is encrypted, the client can't use it
            await AuthCookieHelper.SetCookie(context, claims, authResponse.ExpiresOnUtc);

            // Turn the user data into a JWT and set it in a response header that the client can extract
            var token = AuthCookieHelper.GenerateJwtFromUserInfo(claims, _authOptions.IssuerSigningKey);
            context.Response.Headers.Append("sso_user", token);

            return true;
        }
    }
}

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;

namespace org.cchmc.{{cookiecutter.namespace}}.auth
{
    public static class AuthCookieHelper
    {
        public const string RefreshTokenClaimName = "refresh_token";

        // Claim Name Types for the JWT
        private const string _alertTokenClaimName = "sso_alerts";
        private const string _emailClaimName = "sso_email";
        private const string _titleClaimName = "sso_title";
        private const string _departmentClaimName = "sso_department";
        private const string _empNumClaimName = "sso_empnum";

        private readonly static JsonSerializerOptions jsonOptions = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static async Task SetCookie(HttpContext context, Claim[] claims, DateTime expiresOnUtc)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties()
            {
                ExpiresUtc = expiresOnUtc // Sets this cookie to expire when the refresh token expires
            };

            // Creates an encrypted cookie
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        public static Claim[] GenerateClaims(AuthResponse authResponse, string refreshToken, int cookieExpirationInMinutes)
        {
            Claim[] claims =
            [
                new(ClaimTypes.Name, authResponse.User.UserName),
                new(JwtRegisteredClaimNames.NameId, authResponse.User.Id.ToString() ?? ""),
                new(JwtRegisteredClaimNames.GivenName, authResponse.User.FirstName ?? ""),
                new(JwtRegisteredClaimNames.FamilyName, authResponse.User.LastName ?? ""),
                new(JwtRegisteredClaimNames.UniqueName, authResponse.User.UserName ?? ""),
                new(_empNumClaimName, authResponse.User.EmployeeNumber ?? ""),
                new(_emailClaimName, authResponse.User.Email),
                new(_titleClaimName, authResponse.User.Title),
                new(_departmentClaimName, authResponse.User.Department),
                new(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(cookieExpirationInMinutes).ToString("O")),
                new(RefreshTokenClaimName, refreshToken),
                new(_alertTokenClaimName, JsonSerializer.Serialize(authResponse.ActiveAlerts, jsonOptions)) // these would be at most "expirationInMinutes" minutes behind (aka, 5)
            ];
            authResponse.User.Roles.ForEach(r => claims = [.. claims, new Claim(ClaimTypes.Role, r)]);

            return claims;
        }

        public static string GenerateJwtFromUserInfo(Claim[] claims, string issuerSigningKey)
        {
            // remove refresh token and expiration, which aren't necessary for the UI to know
            var userClaims = claims.Where(c => c.Type != RefreshTokenClaimName && c.Type != ClaimTypes.Expiration).ToArray();

            // sign and create token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
            var signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                SigningCredentials = signInCred
            });
            return token;
        }

        public static AuthResponse GenerateUserFromClaims(List<Claim> claims)
        {
            AuthResponse result = new()
            {
                User = new()
                {
                    FirstName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value,
                    Department = claims.FirstOrDefault(c => c.Type == _departmentClaimName)?.Value,
                    Email = claims.FirstOrDefault(c => c.Type == _emailClaimName)?.Value,
                    EmployeeNumber = claims.FirstOrDefault(c => c.Type == _empNumClaimName)?.Value,
                    Id = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value,
                    LastName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value,
                    Title = claims.FirstOrDefault(c => c.Type == _titleClaimName)?.Value,
                    UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
                },
                ActiveAlerts = claims.Any(c => c.Type == _alertTokenClaimName)
                    ? JsonSerializer.Deserialize<List<SingleSignOnAlert>>(claims.FirstOrDefault(c => c.Type == _alertTokenClaimName).Value, jsonOptions)
                    : []
            };
            return result;
        }

        public static async Task SignOut(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

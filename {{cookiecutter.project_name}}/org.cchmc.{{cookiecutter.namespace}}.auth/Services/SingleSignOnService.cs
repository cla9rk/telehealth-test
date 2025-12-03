using Microsoft.Extensions.Logging;
using org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces;
using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Services
{
    public class SingleSignOnService(HttpClient _client, ILogger<SingleSignOnService> _logger) : ISingleSignOn
    {
        public async Task<AuthResponse> CompleteSignIn(string signinToken, string ipAddress)
        {
            var completeSignIn = new CompleteSignInRequest()
            {
                IpAddress = ipAddress,
                SignInToken = signinToken
            };
            using var stringContent = new StringContent(JsonSerializer.Serialize(completeSignIn), Encoding.UTF8, "application/json");
            return await QuerySso<AuthResponse>(stringContent, "Auth/CompleteSignIn", HttpMethod.Post);
        }

        public async Task<AuthResponse> RefreshToken(string refreshToken, string applicationEnvironmentId, string ipAddress)
        {
            var refreshTokenModel = new RefreshTokenRequest()
            {
                IpAddress = ipAddress,
                ApplicationEnvironmentId = applicationEnvironmentId,
                RefreshToken = refreshToken
            };
            using var stringContent = new StringContent(JsonSerializer.Serialize(refreshTokenModel), Encoding.UTF8, "application/json");
            return await QuerySso<AuthResponse>(stringContent, "Auth/RefreshToken", HttpMethod.Post);
        }

        public async Task<bool> SignOut(string refreshToken, string applicationEnvironmentId, string ipAddress)
        {
            var revokeTokenModel = new RefreshTokenRequest()
            {
                IpAddress = ipAddress,
                ApplicationEnvironmentId = applicationEnvironmentId,
                RefreshToken = refreshToken
            };
            using var stringContent = new StringContent(JsonSerializer.Serialize(revokeTokenModel), Encoding.UTF8, "application/json");
            var result = await QuerySso<string>(stringContent, "Auth/RevokeToken", HttpMethod.Post);
            return result != null;
        }

        public async Task<bool> AcknowledgeAlert(string alertId, string userId)
        {
            Dictionary<string, string> newHeaders = new() { { "SSO-User-Key", userId } };
            return await QuerySso<bool>(null, $"Auth/AcknowledgeAlert/{alertId}", HttpMethod.Post, newHeaders);
        }

        public async Task<string> GetThumbnail(string username)
        {
            return await QuerySso<string>(null, $"Auth/GetThumbnail/{username}", HttpMethod.Get);
        }

        protected async Task<T> QuerySso<T>(StringContent data, string endpoint, HttpMethod method, Dictionary<string, string> additionalHeaders = null)
        {
            T toReturn = default;

            try
            {
                using var requestMessage = new HttpRequestMessage(method, endpoint) { Content = data };

                if (additionalHeaders != null)
                {
                    foreach (var key in additionalHeaders.Keys)
                        requestMessage.Headers.Add(key, additionalHeaders[key]);
                }

                var response = await _client.SendAsync(requestMessage);

                string content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("{statusCode} - {content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                    return toReturn;
                }

                return content is T t
                    ? t
                    : JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred calling SSO");
                throw;
            }
        }
    }
}

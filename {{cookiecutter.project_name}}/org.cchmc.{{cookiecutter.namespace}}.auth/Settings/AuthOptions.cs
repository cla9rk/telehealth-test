using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Settings
{
    public class AuthOptions
    {
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
        public string IssuerSigningKey { get; set; }
        public int ExpirationMinutes { get; set; }
        public string AuthenticationCookieName { get; set; }

        public string SingleSignOnRedirectUrl { get; set; }
        public string RefreshTokenCookieName { get; set; }
        public string ApplicationEnvironmentId { get; set; }
        public string SingleSignOnEndpoint { get; set; }


        public SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.ASCII.GetBytes(IssuerSigningKey));
        public SigningCredentials SigningCredentials => new(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);
    }
}

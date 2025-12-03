using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using org.cchmc.{{cookiecutter.namespace}}.auth.Settings;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Configuration
{
    public class ConfigureCookieOptions(IOptions<AuthOptions> _authOptions) : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            options.ClaimsIssuer = _authOptions.Value.ValidIssuer;
            options.Cookie.IsEssential = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Name = $"{_authOptions.Value.RefreshTokenCookieName}-auth";
        }
    }
}

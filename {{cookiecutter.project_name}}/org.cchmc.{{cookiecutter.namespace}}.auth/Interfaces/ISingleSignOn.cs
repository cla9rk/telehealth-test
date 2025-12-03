using org.cchmc.{{cookiecutter.namespace}}.auth.Models;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Interfaces
{
    public interface ISingleSignOn
    {
        Task<AuthResponse> RefreshToken(string refreshToken, string applicationEnvironmentId, string ipAddress);
        Task<AuthResponse> CompleteSignIn(string signinToken, string ipAddress);
        Task<bool> SignOut(string refreshToken, string applicationEnvironmentId, string ipAddress);
        Task<bool> AcknowledgeAlert(string alertId, string userId);
        Task<string> GetThumbnail(string username);
    }
}

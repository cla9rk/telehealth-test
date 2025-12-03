namespace org.cchmc.{{cookiecutter.namespace}}.auth.Models
{
    public class RefreshTokenRequest
    {
        public string IpAddress { get; set; }
        public string RefreshToken { get; set; }
        public string ApplicationEnvironmentId { get; set; }
    }
}

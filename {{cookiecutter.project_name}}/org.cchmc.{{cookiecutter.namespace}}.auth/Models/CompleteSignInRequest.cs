namespace org.cchmc.{{cookiecutter.namespace}}.auth.Models
{
    public class CompleteSignInRequest
    {
        public string IpAddress { get; set; }
        public string SignInToken { get; set; }
    }
}

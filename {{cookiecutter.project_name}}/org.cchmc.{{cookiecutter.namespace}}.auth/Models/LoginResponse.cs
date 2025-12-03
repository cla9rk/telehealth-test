using System.Collections.Generic;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Models
{
    public class LoginResponse
    {
        public SingleSignOnUser User { get; set; }
        public List<SingleSignOnAlert> Alerts { get; set; }
    }
}

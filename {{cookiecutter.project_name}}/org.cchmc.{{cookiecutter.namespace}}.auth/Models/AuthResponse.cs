using System;
using System.Collections.Generic;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Models
{
    public class AuthResponse
    {
        public SingleSignOnUser User { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresOnUtc { get; set; }
        public List<SingleSignOnAlert> ActiveAlerts { get; set; }
    }
}

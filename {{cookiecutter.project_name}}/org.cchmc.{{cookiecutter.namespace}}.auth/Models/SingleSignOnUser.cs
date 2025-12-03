using System;
using System.Collections.Generic;

namespace org.cchmc.{{cookiecutter.namespace}}.auth.Models
{
    public class SingleSignOnUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ThumbnailPhoto { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string EmployeeNumber { get; set; }
        public DateTime? LastSignIn { get; set; }
        public List<string> Roles { get; set; }
    }
}

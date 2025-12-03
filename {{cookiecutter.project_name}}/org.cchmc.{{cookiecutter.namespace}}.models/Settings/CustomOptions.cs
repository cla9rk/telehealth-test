using System.Collections.Generic;

namespace org.cchmc.{{cookiecutter.namespace}}.models.Settings
{
    public class CustomOptions
    {
        public List<string> DoNotLogMetaDataForPaths { get; set; }
        public HealthCheckConfig HealthCheckConfig { get; set; }
        public string NLogRedactionKey { get; set; } // Should be Base64 and 44 characters long
        public int NLogRedactionKeyId { get; set; } // This is used for correlation purposes and is prepended to the hashed value - should make it unique per application
    }
}

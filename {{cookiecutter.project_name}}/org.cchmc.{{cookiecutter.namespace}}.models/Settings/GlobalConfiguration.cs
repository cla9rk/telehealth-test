namespace org.cchmc.{{cookiecutter.namespace}}.models.Settings
{
    public static class GlobalConfiguration
    {
        public static string DbConnectionString { get; set; }
        public static CustomOptions CustomOptions { get; set; }
    }
}

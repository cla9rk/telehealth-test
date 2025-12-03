using org.cchmc.{{cookiecutter.namespace}}.models.DataTaxonomy;

namespace org.cchmc.{{cookiecutter.namespace}}.models.ViewModels
{
    public class ExampleInput
    {
        [SensitiveData]
        public string SensitiveData { get; set; }
        [PersonalData]
        public string PersonalData { get; set; }
        public string Neither { get; set; }
    }
}

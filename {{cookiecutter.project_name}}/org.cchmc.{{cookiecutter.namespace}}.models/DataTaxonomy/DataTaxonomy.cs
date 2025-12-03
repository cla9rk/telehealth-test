using Microsoft.Extensions.Compliance.Classification;

namespace org.cchmc.{{cookiecutter.namespace}}.models.DataTaxonomy
{
    public static class DataTaxonomy
    {
        public static string LoggingTaxonomyName => typeof(DataTaxonomy).FullName;
        public readonly static DataClassification SensitiveData = new(LoggingTaxonomyName, nameof(SensitiveData));
        public readonly static DataClassification PersonalData = new(LoggingTaxonomyName, nameof(PersonalData));
    }

    public class SensitiveDataAttribute : DataClassificationAttribute
    {
        public SensitiveDataAttribute() : base(DataTaxonomy.SensitiveData) { }
    }

    public class PersonalDataAttribute : DataClassificationAttribute
    {
        public PersonalDataAttribute() : base(DataTaxonomy.PersonalData) { }
    }
}

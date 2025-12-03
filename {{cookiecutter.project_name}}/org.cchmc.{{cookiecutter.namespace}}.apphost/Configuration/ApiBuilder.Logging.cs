using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using org.cchmc.{{cookiecutter.namespace}}.models.DataTaxonomy;
using org.cchmc.{{cookiecutter.namespace}}.models.Settings;
using System;
using System.Text;

namespace org.cchmc.{{cookiecutter.namespace}}.apphost.Configuration
{
    public static partial class ApiBuilder
    {
        public static void RegisterLogging(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.EnableRedaction();
            builder.Services.AddRedaction(x =>
            {
                // Enable the erasing redactor for sensitive data
                x.SetRedactor<ErasingRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData));

                // Enable the HMAC256 redactor for personal data
                x.SetHmacRedactor(hmacOpts =>
                {
                    hmacOpts.Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(GlobalConfiguration.CustomOptions.NLogRedactionKey));
                    hmacOpts.KeyId = GlobalConfiguration.CustomOptions.NLogRedactionKeyId;
                }, new DataClassificationSet(DataTaxonomy.PersonalData));
            });
            builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });
        }
    }
}

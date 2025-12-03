using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using org.cchmc.{{cookiecutter.namespace}}.data.DbContexts;
using org.cchmc.{{cookiecutter.namespace}}.models.Settings;

namespace org.cchmc.{{cookiecutter.namespace}}.data.Configuration
{
    public static partial class ApiBuilder
    {
        public static void AddDbContextAndRepositories(this IServiceCollection services)
        {
            services.AddDbContext<{{cookiecutter.project_camelcase}}DbContext>(options => options.UseSqlServer(GlobalConfiguration.DbConnectionString));
            //services.AddScoped<IBaseRepository<MyObject>, BaseRepository<MyObject, {{cookiecutter.project_camelcase}}DbContext>>();

            services.AddHealthChecks().AddDbContextCheck<{{cookiecutter.project_camelcase}}DbContext>("Database");
        }
    }
}

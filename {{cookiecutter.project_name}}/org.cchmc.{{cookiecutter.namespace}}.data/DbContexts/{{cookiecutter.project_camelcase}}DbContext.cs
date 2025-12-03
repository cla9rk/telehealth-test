using Microsoft.EntityFrameworkCore;

namespace org.cchmc.{{cookiecutter.namespace}}.data.DbContexts
{
    public class {{cookiecutter.project_camelcase}}DbContext(DbContextOptions<{{cookiecutter.project_camelcase}}DbContext> options) : DbContext(options)
    {
        // TODO: Add DbSets here


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Add entity builders here
        }
    }
}

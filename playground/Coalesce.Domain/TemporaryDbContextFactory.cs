using IntelliTect.Coalesce.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Coalesce.Domain;

public class TemporaryDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlServer(DevelopmentConnectionStringLocator.Find(projectDirectorySuffixes: [".Vue3"]));
        return new AppDbContext(builder.Options);
    }
}

using IntelliTect.Coalesce.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Coalesce.Starter.Vue.Data;

public class DevelopmentAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // This is only used by the EF Core CLI tooling (`dotnet ef`).
        // It shouldn't ever be used in code where it might end up running in production.

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlServer(DevelopmentConnectionStringLocator.Find());
        return new AppDbContext(builder.Options);
    }
}

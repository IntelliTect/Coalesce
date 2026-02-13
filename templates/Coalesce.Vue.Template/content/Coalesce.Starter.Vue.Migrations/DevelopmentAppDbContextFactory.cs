using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Models;
using IntelliTect.Coalesce.Helpers;
#if Identity
using Microsoft.AspNetCore.Identity;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Coalesce.Starter.Vue.Migrations;

public class DevelopmentAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // This is only used by the EF Core CLI tooling (`dotnet ef`).
        // It shouldn't ever be used in code where it might end up running in production.

#if Identity
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options
            .UseSqlServer(DevelopmentConnectionStringLocator.Find(), opt => opt
                .MigrationsAssembly(typeof(DevelopmentAppDbContextFactory).Assembly.FullName)
            )
        );
        services.AddIdentity<User, Role>(c =>
        {
            c.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddEntityFrameworkStores<AppDbContext>();

        return services.BuildServiceProvider().GetRequiredService<AppDbContext>();
#else
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlServer(DevelopmentConnectionStringLocator.Find(), opt => opt
            .MigrationsAssembly(typeof(DevelopmentAppDbContextFactory).Assembly.FullName)
        );
        return new AppDbContext(builder.Options);
#endif
    }
}

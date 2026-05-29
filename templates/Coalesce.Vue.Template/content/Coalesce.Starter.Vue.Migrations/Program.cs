using Coalesce.Starter.Vue.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
#if Hangfire
using Hangfire.SqlServer;
#endif

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMigrationDbContext(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

var host = builder.Build();
await host.MigrateDatabaseAsync();

public static partial class Program
{
    public static IServiceCollection AddMigrationDbContext(
        this IServiceCollection services,
        string? connectionString
    )
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseSqlServer(connectionString, opt => opt
                .EnableRetryOnFailure()
                .MigrationsAssembly(typeof(Program).Assembly.FullName)
            )
        );

        services.Configure<IdentityOptions>(o => o.Stores.SchemaVersion = IdentitySchemaVersions.Version3);

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

#if KeepTemplateOnly
        //db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

#else
        await db.Database.MigrateAsync();

#endif
#if Hangfire
        // Install Hangfire storage only after the database has definitely been created.
        // https://github.com/HangfireIO/Hangfire/issues/2139
        SqlServerObjectsInstaller.Install(db.Database.GetDbConnection(), null, true);

#endif
        ActivatorUtilities.GetServiceOrCreateInstance<DatabaseSeeder>(services).Seed();
    }
}
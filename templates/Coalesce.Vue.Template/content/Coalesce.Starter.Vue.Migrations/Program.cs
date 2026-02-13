using Coalesce.Starter.Vue.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
#if Hangfire
using Hangfire.SqlServer;
#endif

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), opt => opt
        .EnableRetryOnFailure()
    )
);

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

#if KeepTemplateOnly
//db.Database.EnsureDeleted();
db.Database.EnsureCreated();

#else
await db.Database.MigrateAsync();

#endif
#if Hangfire
SqlServerObjectsInstaller.Install(db.Database.GetDbConnection(), null, true);

#endif
ActivatorUtilities.GetServiceOrCreateInstance<DatabaseSeeder>(scope.ServiceProvider).Seed();

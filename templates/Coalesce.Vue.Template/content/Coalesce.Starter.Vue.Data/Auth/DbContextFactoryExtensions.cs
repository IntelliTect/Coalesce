namespace Coalesce.Starter.Vue.Data.Auth;

public static class DbContextFactoryExtensions
{
    public static IEnumerable<string> GetTenantIds(this IDbContextFactory<AppDbContext> factory)
    {
        using var db = factory.CreateDbContext();

        return db.Tenants.Select(t => t.TenantId).ToList();
    }

    public static AppDbContext CreateDbContext(this IDbContextFactory<AppDbContext> factory, string tenantId)
    {
        var db = factory.CreateDbContext();
        db.TenantId = tenantId;
        return db;
    }
}

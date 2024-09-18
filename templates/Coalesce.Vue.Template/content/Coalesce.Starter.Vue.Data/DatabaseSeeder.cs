using Coalesce.Starter.Vue.Data.Models;

namespace Coalesce.Starter.Vue.Data;

public class DatabaseSeeder(AppDbContext db)
{
    public void Seed()
    {
#if Tenancy
        // TODO: create initial tenant only if not externally sourced
        if (!db.Tenants.Any())
        {
            var tenant = new Tenant { Name = "Demo Tenant" };
            db.Add(tenant);
            db.SaveChanges();

            SeedTenant(tenant.TenantId);
        }
#elif Identity
        SeedRoles();
#endif
    }

#if Tenancy
    public void SeedTenant(int tenantId)
    {
        db.TenantId = tenantId;

#if Identity
        SeedRoles();
#endif
    }
#endif

#if Identity
    private void SeedRoles()
    {
        if (!db.Roles.Any())
        {
            db.Roles.Add(new()
            {
                Permissions = Enum.GetValues<Permission>().ToList(),
                Name = "Admin",
                NormalizedName = "ADMIN",
            });

            // NOTE: In a permissions-based authorization system,
            // roles can freely be created by administrators in the admin pages.

            db.SaveChanges();
        }
    }
#endif
}

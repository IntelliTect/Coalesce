using Coalesce.Starter.Vue.Data.Models;

namespace Coalesce.Starter.Vue.Data;

public class DatabaseSeeder(AppDbContext db)
{
    public void Seed()
    {
#if Tenancy
#if (!TenantCreateExternal && !TenantCreateSelf)
        if (!db.Tenants.Any())
        {
            var tenant = new Tenant { Name = "Demo Tenant" };
            db.Add(tenant);
            db.SaveChanges();

            SeedNewTenant(tenant);
        }
#endif
#elif Identity
        SeedRoles();
#endif
    }

#if Tenancy
    public void SeedNewTenant(Tenant tenant, string? userId = null)
    {
        var tenantId = tenant.TenantId;
        db.TenantId = tenantId;

#if Identity
        SeedRoles();

        if (userId is not null)
        {
            // Give the first user in the tenant all roles so there is an initial admin.
            db.AddRange(db.Roles.Select(r => new UserRole { Role = r, UserId = userId }));
            db.Add(new TenantMembership { UserId = userId });
        }

        db.SaveChanges();
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

            // NOTE: In this application's permissions-based authorization system,
            // additional roles can freely be created by administrators.
            // You don't have to seed every possible role.

            db.SaveChanges();
        }
    }
#endif
}

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

            SeedNewTenant(tenant.TenantId);
        }
#elif Identity
        SeedRoles();
#endif
    }

#if Tenancy
    public void SeedNewTenant(string tenantId, string? userId = null)
    {
        db.TenantId = tenantId;

#if Identity
        SeedRoles();

        if (userId is not null)
        {
            // Give the first user in the tenant all roles so there is an initial admin.
            db.AddRange(db.Roles.Select(r => new UserRole { Role = r, UserId = userId, TenantId = db.TenantId }));
            db.Add(new TenantMembership { TenantId = tenantId, UserId = userId });
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
                TenantId = db.TenantIdOrThrow
            });

            // NOTE: In a permissions-based authorization system,
            // roles can freely be created by administrators in the admin pages.

            db.SaveChanges();
        }
    }
#endif
}

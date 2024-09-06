namespace Coalesce.Starter.Vue.Data;

public class DatabaseSeeder(AppDbContext db)
{
    public void Seed()
    {
#if Identity
        SeedRoles();
#endif
    }

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

using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Read(AllowAuthenticated)]
[Edit(Roles = nameof(Permission.Admin))]
[Create(DenyAll)]
[Delete(DenyAll)]
#if TenantCreateExternal
[Index(nameof(ExternalId), IsUnique = true)]
#endif
[Display(Name = "Organization")]
public class Tenant
{
    public string TenantId { get; set; } = Guid.NewGuid().ToString();

    public required string Name { get; set; }

#if TenantCreateExternal
    [Read]
    [Description("The external origin of this tenant. Other users who sign in with accounts from this external source will automatically join this organization.")]
    public string? ExternalId { get; set; }
#endif

    [DefaultDataSource]
    public class DefaultSource(CrudContext<AppDbContext> context) : AppDataSource<Tenant>(context)
    {
        public override IQueryable<Tenant> GetQuery(IDataSourceParameters parameters)
        {
            // Only allow the current tenant to be read and modified.
            return base.GetQuery(parameters)
                .Where(t => t.TenantId == User.GetTenantId());
        }
    }

    public class GlobalAdminSource(CrudContext<AppDbContext> context) : AppDataSource<Tenant>(context)
    {
        public override IQueryable<Tenant> GetQuery(IDataSourceParameters parameters)
        {
            if (!User.IsInRole(AppClaimValues.GlobalAdminRole))
            {
                return Enumerable.Empty<Tenant>().AsQueryable();
            }

            return base.GetQuery(parameters);
        }
    }

#if TenantCreateAdmin
    [Coalesce, Execute(AppClaimValues.GlobalAdminRole)]
    public static async Task<ItemResult> Create(
        AppDbContext db,
        [Inject] InvitationService invitationService,
        [Display(Name = "Org Name")] string name,
        [DataType(DataType.EmailAddress)] string adminEmail
    )
    {
        Tenant tenant = new() { Name = name };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        db.ForceSetTenant(tenant.TenantId);
        new DatabaseSeeder(db).SeedNewTenant(tenant);

        return await invitationService.CreateAndSendInvitation(adminEmail, db.Roles.ToArray());
    }
#endif
}

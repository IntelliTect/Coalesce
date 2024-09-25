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
            // By default, only allow the current tenant to be read and modified.
            // If desired, this can be made more permissive.
            return base.GetQuery(parameters)
                .Where(t => t.TenantId == User.GetTenantId());
        }
    }
}

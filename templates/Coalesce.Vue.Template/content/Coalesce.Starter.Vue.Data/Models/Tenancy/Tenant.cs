using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Read(AllowAuthenticated)]
[Edit(Roles = nameof(Permission.Admin))]
[Create(DenyAll)]
[Delete(DenyAll)]
[Index(nameof(ExternalId), IsUnique = true)]
[Display(Name = "Organization")]
public class Tenant
{
    public string TenantId { get; set; } = Guid.NewGuid().ToString();

    public required string Name { get; set; }

    [Read]
    [Description("The external origin of this tenant. Other users who sign in with accounts from this external source will automatically join this organization.")]
    public string? ExternalId { get; set; }

    [DefaultDataSource]
    public class DefaultSource(CrudContext<AppDbContext> context) : AppDataSource<Tenant>(context)
    {
        public override IQueryable<Tenant> GetQuery(IDataSourceParameters parameters)
        {
            return base.GetQuery(parameters)
                .Where(t => t.TenantId == User.GetTenantId());
        }
    }
}

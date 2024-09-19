

namespace Coalesce.Starter.Vue.Data.Models;

[Read(AllowAuthenticated)]
[Edit(Roles = nameof(Permission.Admin))]
[Create(DenyAll)]
[Delete(DenyAll)]
[Index(nameof(ExternalId), IsUnique = true)]
public class Tenant
{
    public int TenantId { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// An identifier representing the external source of the tenant.
    /// </summary>
    [Read]
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

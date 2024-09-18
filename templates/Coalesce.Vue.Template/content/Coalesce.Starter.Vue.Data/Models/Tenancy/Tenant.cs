namespace Coalesce.Starter.Vue.Data.Models;

[InternalUse]
[Index(nameof(ExternalId), IsUnique = true)]
public class Tenant
{
    public int TenantId { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// An identifier representing the external source of the tenant.
    /// </summary>
    public string? ExternalId { get; set; }

    public void AddInitialData(AppDbContext db)
    {
    }
}

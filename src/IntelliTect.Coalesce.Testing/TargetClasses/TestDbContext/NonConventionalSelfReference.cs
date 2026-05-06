#nullable enable

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

public class SelfOwnedTenant
{
    public int Id { get; set; }

    public int? TenantId { get; set; }
    public SelfOwnedTenant? OwnerTenant { get; set; }
}

public class SelfOwnedTenantConsumer
{
    public int Id { get; set; }

    public int? TenantId { get; set; }
    public SelfOwnedTenant? OwnerTenant { get; set; }
}

using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

[Create(SecurityPermissionLevels.DenyAll)]
[Edit(SecurityPermissionLevels.DenyAll)]
public class RequiredInternalUseModel
{
    public int Id { get; set; }

    [InternalUse]
    public required string InternalRequiredProp { get; init; }
}

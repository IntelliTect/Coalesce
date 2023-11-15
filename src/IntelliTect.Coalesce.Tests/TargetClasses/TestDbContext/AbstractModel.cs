using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    // Attributes defined on the base class to test attribute inheritance
    [Read(Roles = "ReadRole")]
    [Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    [Edit(PermissionLevel = SecurityPermissionLevels.AllowAll)]
    public abstract class AbstractModel
    {
        public int Id { get; set; }

        public string Discriminatior { get; set; }
    }

    [Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    public class AbstractImpl : AbstractModel
    {
        public string ImplOnlyField { get; set; }
    }
}

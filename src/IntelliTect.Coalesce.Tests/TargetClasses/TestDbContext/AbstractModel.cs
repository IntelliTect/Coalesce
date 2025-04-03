using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    // Attributes defined on the base class to test attribute inheritance
    [Read(Roles = "ReadRole")]
    [Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    [Edit(PermissionLevel = SecurityPermissionLevels.AllowAll)]
    public abstract class AbstractModel
    {
        public int Id { get; set; }

        public string Discriminator { get; set; }

        [ManyToMany("People")]
        public List<AbstractModelPerson> AbstractModelPeople { get; set; }

        [Coalesce]
        public int GetId() => Id;

        [Coalesce]
        public static int GetCount(AppDbContext db) => db.AbstractModels.Count();

        [Coalesce]
        public static AbstractModel EchoAbstractModel(AbstractModel model) => model;
    }

    [Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    public class AbstractImpl1 : AbstractModel
    {
        public string Impl1OnlyField { get; set; }

        public int? ParentId { get; set; }
        public AbstractModel Parent { get; set; }
    }

    [Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    public class AbstractImpl2 : AbstractModel
    {
        public string Impl2OnlyField { get; set; }
    }

    public class AbstractModelPerson
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person Person { get; set; }

        public int AbstractModelId { get; set; }
        public AbstractModel AbstractModel { get; set; }
    }
}

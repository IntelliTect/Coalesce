using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class OneToOneParent
    {
        public int Id { get; set; }

       // [InverseProperty(nameof(Child1.Parent))]
        public OneToOneChild1 Child1 { get; set; }

        [InverseProperty(nameof(Child2.Parent))] // Can be specified, but not required.
        public OneToOneChild2 Child2 { get; set; }
    }

    public class OneToOneChild1
    {
        [Key, ForeignKey("Parent")]
        public int ParentId { get; set; }
        public OneToOneParent Parent { get; set; }
    }

    public class OneToOneChild2
    {
        [Key, ForeignKey("Parent")]
        public int ParentId { get; set; }
        public OneToOneParent Parent { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

public class OneToOneParent
{
    public int Id { get; set; }

    // [InverseProperty(nameof(SharedKeyChild1.Parent))]
    public OneToOneSharedKeyChild1 SharedKeyChild1 { get; set; }

    [InverseProperty(nameof(SharedKeyChild2.Parent))] // Can be specified, but not required.
    public OneToOneSharedKeyChild2 SharedKeyChild2 { get; set; }

    // Coalesce can't model reference navigations of separate-key one-to-one relationships
    // from the side that doesn't own the FK, since Coalesce can only represent
    // a referenceNavigation that has a corresponding foreignKey on the same type.
    // This will emit as a "value" role, despite having InverseProperty
    // which would normally make this a referenceNavigation.
    [InverseProperty(nameof(SeparateKeyChild.Parent))]
    public OneToOneSeparateKeyChild SeparateKeyChild { get; set; }

    // This should also emit as a "value" role.
    public OneToOneSeparateKeyChild SeparateKeyChildNoIp { get; set; }

    public List<OneToOneManyChildren> ManyChildren { get; set; } = [];
}

public class OneToOneSharedKeyChild1
{
    [Key, ForeignKey("Parent")]
    public int ParentId { get; set; }
    public OneToOneParent Parent { get; set; }
}

public class OneToOneSharedKeyChild2
{
    [Key, ForeignKey("Parent")]
    public int ParentId { get; set; }
    public OneToOneParent Parent { get; set; }
}

[Index(nameof(ParentId), IsUnique = true)]
public class OneToOneSeparateKeyChild
{
    public int Id { get; set; }

    [ForeignKey("Parent")]
    public int ParentId { get; set; }
    public OneToOneParent Parent { get; set; }
}

public class OneToOneManyChildren
{
    public int Id { get; set; }

    public int OneToOneParentId { get; set; }
    public OneToOneParent OneToOneParent { get; set; }
}

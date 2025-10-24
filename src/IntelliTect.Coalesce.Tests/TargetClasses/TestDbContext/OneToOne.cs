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

    public OneToOneSeparateKeyChild SeparateKeyChild { get; set; }

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

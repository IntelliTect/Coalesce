using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

public class RecursiveHierarchy
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? ParentId { get; set; }
    public RecursiveHierarchy? Parent { get; set; }

    [InverseProperty(nameof(Parent))]
    public List<RecursiveHierarchy> Children { get; set; } = new();
}

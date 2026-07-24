using IntelliTect.Coalesce;
using System;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

/// <summary>
/// Simple model target for testing. See <see cref="NotMarkedAsSimpleModel"/> for comparison.
/// This should reference <see cref="System.Threading.CancellationToken"/> properly.
/// </summary>
[Coalesce, SimpleModel]
public class SimpleModelTarget
{
    public int Id { get; set; }
    
    /// <summary>
    /// The name property. Similar to <see cref="CoalesceOnlyTarget.Description"/>.
    /// </summary>
    public string Name { get; set; }
    
    public DateTime CreatedDate { get; set; }
}

public class NotMarkedAsSimpleModel
{
    public int Id { get; set; }
    public string Value { get; set; }
}

[Coalesce]
public class CoalesceOnlyTarget
{
    public int Id { get; set; }
    public string Description { get; set; }
}

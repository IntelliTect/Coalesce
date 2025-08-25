using IntelliTect.Coalesce;
using System;

namespace IntelliTect.Coalesce.Tests.TargetClasses;

[Coalesce]
public class SimpleModelTarget
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class NotMarkedAsSimpleModel
{
    public int Id { get; set; }
    public string Value { get; set; }
}
using System;

namespace IntelliTect.Coalesce.TypeDefinition.Enums;

[Flags]
public enum PropertyRole
{
    Value,
    ReferenceNavigation,
    CollectionNavigation,
    PrimaryKey,
    ForeignKey,
}

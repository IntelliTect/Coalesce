using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition.Enums
{
    [Flags]
    public enum PropertyRole
    {
        Value,
        ReferenceNavigation,
        CollectionNavigation,
        PrimaryKey,
        ForeignKey,
    }
}

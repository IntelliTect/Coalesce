using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class EnumMember
    {
        public EnumMember(
            string name, 
            object value, 
            string displayName,
            string? description
        )
        {
            Name = name;
            DisplayName = displayName;
            Value = value;
            Description = description;
        }

        public string Name { get; }
        public object Value { get; }
        public string DisplayName { get; }
        public string? Description { get; }
    }
}

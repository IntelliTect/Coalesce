using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class EnumMember
    {
        public EnumMember(string name, object value, string displayName)
        {
            Name = name;
            DisplayName = displayName;
            Value = value;
        }

        public string Name { get; }
        public object Value { get; }
        public string DisplayName { get; }
    }
}

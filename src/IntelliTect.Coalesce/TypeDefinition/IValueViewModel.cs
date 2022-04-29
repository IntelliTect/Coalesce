using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public interface IValueViewModel : IAttributeProvider
    {
        string Name { get; }

        string JsVariable { get; }

        string DisplayName { get; }
        
        string? Description { get; }

        /// <summary>
        /// Gets the raw, unaltered type of the value.
        /// </summary>
        TypeViewModel Type { get; }

        /// <summary>
        /// Gets the type without any collection around it.
        /// </summary>
        TypeViewModel PureType { get; }
    }
}

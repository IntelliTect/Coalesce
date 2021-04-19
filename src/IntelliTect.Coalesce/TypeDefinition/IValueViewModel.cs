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

        TypeViewModel Type { get; }

        TypeViewModel PureType { get; }
    }
}

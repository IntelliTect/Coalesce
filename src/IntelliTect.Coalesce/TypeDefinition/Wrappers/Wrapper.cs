using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal abstract class Wrapper : IAttributeProvider
    {
        public abstract string Name { get; }

        public abstract object GetAttributeValue<TAttribute>(string valueName)
            where TAttribute : Attribute;

        public abstract bool HasAttribute<TAttribute>()
            where TAttribute : Attribute;

        public virtual AttributeWrapper GetSecurityAttribute<TAttribute>() where TAttribute : SecurityAttribute
        {
            throw new NotImplementedException();
        }
    }
}

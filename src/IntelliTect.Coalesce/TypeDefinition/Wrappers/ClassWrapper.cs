using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal abstract class ClassWrapper : Wrapper
    {
        public abstract List<PropertyWrapper> Properties { get; }
        public abstract List<MethodWrapper> RawMethods { get; }

        public abstract string Comment { get; }

        public abstract bool IsComplexType { get; }

        public Type Info { get; internal set; }
        public ITypeSymbol Symbol { get; internal set; }

        public abstract string Namespace { get; }

        public List<MethodWrapper> Methods
        {
            get
            {
                var result = new List<MethodWrapper>();
                List<string> exclude = new List<string>()
                    {
                        "Include", "IncludeExternal", "ToString", "Equals", "GetHashCode", "GetType"
                    };
                foreach (MethodWrapper method in RawMethods.Where(f => !exclude.Contains(f.Name)))
                {
                    result.Add(method);
                }
                return result;
            }
        }
    }
}
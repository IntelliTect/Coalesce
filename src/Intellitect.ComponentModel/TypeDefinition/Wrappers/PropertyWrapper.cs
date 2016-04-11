using Intellitect.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.TypeDefinition.Wrappers
{
    internal abstract class PropertyWrapper : Wrapper
    {
        public abstract TypeWrapper Type { get; }

        public abstract string Comment { get; }

        public abstract bool CanRead { get; }
        public abstract bool CanWrite { get; }
        public abstract bool IsReadOnly { get; }

        // Exposed only on the reflection version.
        public abstract PropertyInfo PropertyInfo { get; }

        public abstract bool IsVirtual { get; }
        public abstract bool IsStatic { get; }

        public bool IsInternalUse { get
            {
                return HasAttribute<InternalUseAttribute>();
            }
        }
    }


}

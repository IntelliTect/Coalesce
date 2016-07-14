using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal abstract class MethodWrapper: Wrapper
    {
        public abstract bool IsStatic { get; }

        public abstract string Comment { get; }

        public abstract TypeWrapper ReturnType { get; }

        public abstract IEnumerable<ParameterViewModel> Parameters { get; }

        public bool IsInternalUse
        {
            get
            {
                return HasAttribute<InternalUseAttribute>();
            }
        }
    }
}

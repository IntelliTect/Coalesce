using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal abstract class ParameterWrapper : Wrapper
    {
        protected internal ParameterInfo Info { get; internal set; }
        protected internal IParameterSymbol Symbol { get; internal set; }

        protected internal TypeViewModel Type { get; internal set; }
    }
}

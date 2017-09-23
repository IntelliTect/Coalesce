using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Base
{
    public abstract class TypeLocator
    {
        public abstract TypeViewModel FindType(string typeName, bool throwWhenNotFound = true);
    }
}

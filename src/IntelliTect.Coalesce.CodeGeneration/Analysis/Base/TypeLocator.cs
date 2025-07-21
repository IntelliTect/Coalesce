﻿using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Base;

public abstract class TypeLocator
{
    public abstract TypeViewModel FindType(string typeName, bool throwWhenNotFound = true);

    public abstract IEnumerable<TypeViewModel> FindDerivedTypes(string typeName, bool throwWhenNotFound = true);
}

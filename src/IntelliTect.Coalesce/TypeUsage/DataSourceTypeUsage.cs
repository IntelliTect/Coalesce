using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeUsage
{
    public class DataSourceTypeUsage
    {
        public DataSourceTypeUsage(TypeViewModel typeViewModel, ClassViewModel sourceFor)
        {
            TypeViewModel = typeViewModel;
            SourceFor = sourceFor;
        }

        public TypeViewModel TypeViewModel { get; }
        public ClassViewModel SourceFor { get; }
    }
}

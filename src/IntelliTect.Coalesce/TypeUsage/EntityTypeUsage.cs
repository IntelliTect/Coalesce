using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeUsage
{
    public class EntityTypeUsage
    {
        public EntityTypeUsage(DbContextTypeUsage context, ClassViewModel classViewModel, string contextPropertyName)
        {
            ClassViewModel = classViewModel;
            ContextPropertyName = contextPropertyName;
        }

        public ClassViewModel ClassViewModel { get; }

        public string ContextPropertyName { get; }
    }
}

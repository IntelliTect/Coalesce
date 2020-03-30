using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.TypeUsage
{
    public class EntityTypeUsage
    {
        public EntityTypeUsage(DbContextTypeUsage context, TypeViewModel typeViewModel, string contextPropertyName)
        {
            Context = context;
            TypeViewModel = typeViewModel;
            ClassViewModel = typeViewModel.ClassViewModel;
            ContextPropertyName = contextPropertyName;
        }

        public DbContextTypeUsage Context { get; }

        public TypeViewModel TypeViewModel { get; }

        public ClassViewModel ClassViewModel { get; } 

        public string ContextPropertyName { get; }
    }
}

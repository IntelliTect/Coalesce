using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.TypeUsage
{
    public class DbContextTypeUsage
    {
        public DbContextTypeUsage(ClassViewModel classViewModel)
        {
            ClassViewModel = classViewModel;
            Entities = classViewModel
                .ClientProperties

                // Only use props that were explicitly declared on the dbcontext (and not a base class)
                // as well as any props that aren't in the Microsoft namespace.
                // This prevents us from picking up things from Microsoft.AspNetCore.Identity.EntityFrameworkCore
                // that don't have keys & other properties that Coalesce can work with.
                .Where(p => p.Parent.Equals(classViewModel) || !p.PureType.FullNamespace.StartsWith(nameof(Microsoft) + "."))

                .Where(p => p.Type.IsA(typeof(DbSet<>)))
                .Select(p => new EntityTypeUsage(this, p.PureType, p.Name))
                .ToList()
                .AsReadOnly();

        }

        public ClassViewModel ClassViewModel { get; }

        public IReadOnlyList<EntityTypeUsage> Entities { get; }

        public override bool Equals(object? obj) => obj is DbContextTypeUsage that && that.ClassViewModel.Equals(ClassViewModel);

        public override int GetHashCode() => ClassViewModel.GetHashCode();
    }
}

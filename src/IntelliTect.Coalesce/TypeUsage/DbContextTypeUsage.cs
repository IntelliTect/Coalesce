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
                .Where(p => p.Type.IsA(typeof(DbSet<>)))
                .Select(p =>
                {
                    var usage = new EntityTypeUsage(this, p.PureType.ClassViewModel, p.Name);

                    // TODO: eliminate these.
                    usage.ClassViewModel.OnContext = true;
                    usage.ClassViewModel.HasDbSet = true;

                    return usage;
                })
                .ToList()
                .AsReadOnly();

        }

        public ClassViewModel ClassViewModel { get; }

        public IReadOnlyList<EntityTypeUsage> Entities { get; }

        public override bool Equals(object obj) => obj is DbContextTypeUsage that && that.ClassViewModel.Equals(ClassViewModel);

        public override int GetHashCode() => ClassViewModel.GetHashCode();
    }
}

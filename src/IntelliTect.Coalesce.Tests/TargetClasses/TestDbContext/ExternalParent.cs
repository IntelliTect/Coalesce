using System;
using System.Collections.Generic;
using System.Text;

#nullable enable annotations
#nullable disable warnings

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class ExternalParent
    {
        public int[] ValueArray { get; set; }
        public int?[] ValueNullableArray { get; set; }
        public int[]? ValueArrayNullable { get; set; }
        public ICollection<int> ValueICollection { get; set; }
        public ICollection<int?> ValueNullableICollection { get; set; }
        public ICollection<int>? ValueICollectionNullable { get; set; }
        public ExternalChild[] RefArray { get; set; }
        public ExternalChild?[] RefNullableArray { get; set; }
        public ExternalChild[]? RefArrayNullable { get; set; }
        public ICollection<ExternalChild> RefICollection { get; set; }
        public ICollection<ExternalChild?> RefNullableICollection { get; set; }
        public ICollection<ExternalChild>? RefICollectionNullable { get; set; }
    }
}

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
        public List<int> ValueList { get; set; }

        public ICollection<string> StringICollection { get; set; }
        public List<string> StringList { get; set; }

        public ExternalChild[] RefArray { get; set; }
        public ExternalChild?[] RefNullableArray { get; set; }
        public ExternalChild[]? RefArrayNullable { get; set; }

        public ICollection<ExternalChild> RefICollection { get; set; }
        public ICollection<ExternalChild?> RefNullableICollection { get; set; }
        public ICollection<ExternalChild>? RefICollectionNullable { get; set; }

        public List<ExternalChild> RefList { get; set; }
        public List<ExternalChild?> RefNullableList { get; set; }
        public List<ExternalChild>? RefListNullable { get; set; }
    }

    public class ExternalChild
    {
        public string Value { get; set; }
    }

    public class ExternalParentAsInputOnly
    {
        public ExternalChildAsInputOnly Child { get; set; }
    }
    public class ExternalChildAsInputOnly
    {
        public string Value { get; set; }
        public ExternalParentAsInputOnly Recursive { get; set; }
    }

    public class ExternalParentAsOutputOnly
    {
        public ExternalChildAsOutputOnly Child { get; set; }
    }
    public class ExternalChildAsOutputOnly 
    { 
        public string Value { get; set; }
        public ExternalParentAsOutputOnly Recursive { get; set; }
    }

    public class OutputOnlyExternalTypeWithoutDefaultCtor
    {
        public OutputOnlyExternalTypeWithoutDefaultCtor(string foo)
        {
            Bar = foo;
            Baz = foo;
        }

        public string Bar { get; }

        public string Baz { get; internal set; }
    }
}

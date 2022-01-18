using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class ComplexModel
    {
        [DefaultOrderBy(FieldOrder = 2)]
        public int ComplexModelId { get; set; }

        [InverseProperty(nameof(Test.ComplexModel))]
        public ICollection<Test> Tests { get; set; }

        public int SingleTestId { get; set; }
        public Test SingleTest { get; set; }

        [Search]
        public DateTimeOffset DateTimeOffset { get; set; }

        public DateTimeOffset? DateTimeOffsetNullable { get; set; }

        [Search]
        public DateTime DateTime { get; set; }

        public DateTime? DateTimeNullable { get; set; }


        internal string InternalProperty { get; set; }

        [InternalUse]
        public string InternalUseProperty { get; set; }

        [NotMapped]
        public string UnmappedSettableString { get; set; }

        [Read(RoleNames.Admin)]
        public string AdminReadableString { get; set; }

        [Read(RoleNames.Admin)]
        public int? AdminReadableReferenceNavigationId { get; set; }

        [Read(Roles = RoleNames.Admin)]
        [ForeignKey(nameof(AdminReadableReferenceNavigationId))]
        public ComplexModel AdminReadableReferenceNavigation { get; set; }

        public int? ReferenceNavigationId { get; set; }
        [ForeignKey(nameof(ReferenceNavigationId))]
        public ComplexModel ReferenceNavigation { get; set; }


        // Default searchable property
        [DefaultOrderBy(FieldOrder = 1)]
        public string Name { get; set; }


        public string String { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Equals)]
        public string StringSearchedEqualsInsensitive { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.EqualsNatural)]
        public string StringSearchedEqualsNatural { get; set; }

        public int Int { get; set; }
        public int? IntNullable { get; set; }
        public decimal? DecimalNullable { get; set; }
        public long Long { get; set; }
        public Guid Guid { get; set; }
        public Guid? GuidNullable { get; set; }


        public Case.Statuses? EnumNullable { get; set; }

        [NotMapped]
        public IReadOnlyCollection<string> ReadOnlyPrimitiveCollection { get; set; }
        [NotMapped]
        public ICollection<string> MutablePrimitiveCollection { get; set; }
        [NotMapped]
        public IEnumerable<string> PrimitiveEnumerable { get; set; }


        // Add other kinds of properties, relationships, etc... as needed.

        [Coalesce, Execute]
        public ExternalParent MethodWithExternalTypeParams(
            ExternalParent single, 
            ICollection<ExternalParent> collection
        )
        {
            return collection.FirstOrDefault() ?? single;
        }
    }
}

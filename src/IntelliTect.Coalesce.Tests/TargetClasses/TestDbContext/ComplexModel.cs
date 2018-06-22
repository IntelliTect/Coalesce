using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    public class ComplexModel
    {
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

        [Read(Roles = "Admin")]
        public string AdminReadableString { get; set; }

        // Default searchable property
        public string Name { get; set; }


        public string String { get; set; }
        public int Int { get; set; }
        public long Long { get; set; }
        public Guid Guid { get; set; }

        // Add other kinds of properties, relationships, etc... as needed.
    }
}

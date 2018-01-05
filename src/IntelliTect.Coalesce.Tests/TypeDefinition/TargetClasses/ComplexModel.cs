using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.TargetClasses
{
    public class ComplexModel
    {
        public int ComplexModelId { get; set; }

        public ICollection<Test> Tests { get; set; }

        public int SingleTestId { get; set; }
        public Test SingleTest { get; set; }

        [Search]
        public DateTimeOffset DateTimeOffset { get; set; }

        public DateTimeOffset? DateTimeOffsetNullable { get; set; }

        [Search]
        public DateTime DateTime { get; set; }

        public DateTime? DateTimeNullable { get; set; }

        // Add other kinds of properties, relationships, etc... as needed.
    }
}

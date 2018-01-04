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

        // Add other kinds of properties, relationships, etc... as needed.
    }
}

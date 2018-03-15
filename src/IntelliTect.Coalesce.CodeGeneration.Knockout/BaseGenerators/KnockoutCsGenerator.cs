using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators
{
    public abstract class KnockoutCsGenerator : StringBuilderCSharpGenerator<ClassViewModel>
    {
        public KnockoutCsGenerator(GeneratorServices services) : base(services) { }
    }
}

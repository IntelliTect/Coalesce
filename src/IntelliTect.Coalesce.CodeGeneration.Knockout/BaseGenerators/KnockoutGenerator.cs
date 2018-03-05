using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators
{
    public abstract class KnockoutGenerator : StringBuilderFileGenerator<ClassViewModel>
    {
        public KnockoutGenerator(GeneratorServices services) : base(services) {
            Services = services;
        }

        public GeneratorServices Services { get; }
        public GenerationContext GenerationContext => Services.GenerationContext;
        public string AreaName => GenerationContext.AreaName;
        public string ModulePrefix => GenerationContext.TypescriptModulePrefix;
    }
}

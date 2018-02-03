using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class VueSuite : CompositeGenerator<ReflectionRepository>, IRootGenerator
    {
        public VueSuite(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            yield return Generator<Scripts>()
                .WithModel(Model)
                .AppendOutputPath("ClientApp");

            yield return Generator<Models>()
                .WithModel(Model)
                .AppendOutputPath("Models");

            yield return Generator<Controllers>()
                .WithModel(Model);
        }
    }
}

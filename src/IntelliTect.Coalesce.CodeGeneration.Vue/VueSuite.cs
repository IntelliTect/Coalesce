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
            // Output to "src" is the default folder for vue-cli based projects,
            // which is the golden standard to support. Anything other than this
            // can still be changed with configuration.
            yield return Generator<Scripts>()
                .WithModel(Model)
                .AppendOutputPath("src");

            yield return Generator<Models>()
                .WithModel(Model)
                .AppendOutputPath("Models");

            yield return Generator<Controllers>()
                .WithModel(Model);
        }
    }
}

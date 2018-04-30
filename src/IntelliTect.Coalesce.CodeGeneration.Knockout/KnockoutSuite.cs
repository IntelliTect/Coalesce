using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KnockoutSuite : CompositeGenerator<ReflectionRepository>, IRootGenerator
    {
        public KnockoutSuite(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            yield return Generator<Scripts>()
                .WithModel(Model)
                .AppendOutputPath("Scripts");

            yield return Generator<Views>()
                .WithModel(Model)
                .AppendOutputPath("Views");

            yield return Generator<Models>()
                .WithModel(Model)
                .AppendOutputPath("Models");

            yield return Generator<Controllers>()
                .WithModel(Model);
        }
    }
}

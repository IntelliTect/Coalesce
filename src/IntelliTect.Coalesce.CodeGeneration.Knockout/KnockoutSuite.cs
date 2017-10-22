using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KnockoutSuite : CompositeGenerator<List<ClassViewModel>>
    {
        public KnockoutSuite(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            yield return Generator<Scripts>()
                .WithModel(Model)
                .AppendOutputPath("Scripts");

            yield return Generator<Views>()
                .WithModel(Model)
                .AppendOutputPath("Views/Generated");

            yield return Generator<Models>()
                .WithModel(Model)
                .AppendOutputPath("Models/Generated");

            yield return Generator<Controllers>()
                .WithModel(Model);
        }
    }
}

using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KnockoutSuite : CompositeGenerator
    {
        public KnockoutSuite(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            var model = ReflectionRepository.Models.FirstOrDefault(m => m.Name == "Case");
            yield return Generator<KoViewModel>()
                .WithModel(model)
                .AppendOutputPath($"Scripts/Generated/Ko.{model.Name}.ts");
        }
    }
}

using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class Scripts : CompositeGenerator<List<ClassViewModel>>
    {
        public Scripts(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            string Partial(ClassViewModel model) => model.HasTypeScriptPartial ? ".Partial" : "";

            foreach (var model in this.Model)
            {
                if (model.OnContext || model.IsDto)
                {
                    yield return Generator<KoViewModel>()
                        .WithModel(model)
                        .AppendOutputPath($"Generated/Ko.{model.Name}{Partial(model)}.ts");
                    yield return Generator<KoListViewModel>()
                        .WithModel(model)
                        .AppendOutputPath($"Generated/Ko.{model.Name}List.ts");
                }
                else
                {
                    yield return Generator<KoExternalType>()
                        .WithModel(model)
                        .AppendOutputPath($"Generated/Ko.{model.Name}{Partial(model)}.ts");
                }

                if (model.HasTypeScriptPartial)
                {
                    yield return Generator<KoTsPartialStub>()
                        .WithModel(model)
                        .AppendOutputPath($"Partials/Ko.{model.Name}{Partial(model)}.partial.ts");
                }
            }
        }
    }
}

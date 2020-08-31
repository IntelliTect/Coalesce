using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class Controllers : CompositeGenerator<ReflectionRepository>
    {
        public Controllers(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<ICleaner> GetCleaners()
        {
            yield return Cleaner<DirectoryCleaner>()
                .AppendTargetPath("Api/Generated");
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            foreach (var model in Model.CrudApiBackedClasses)
            {
                if (model.WillCreateApiController)
                {
                    yield return Generator<ModelApiController>()
                        .WithModel(model)
                        .AppendOutputPath($"Api/Generated/{model.ClientTypeName}Controller.g.cs");
                }
            }

            foreach (var model in Model.Services)
            {
                yield return Generator<ServiceApiController>()
                    .WithModel(model)
                    .AppendOutputPath($"Api/Generated/{model.ApiControllerClassName}.g.cs");
            }
        }
    }
}

using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class Controllers : CompositeGenerator<List<ClassViewModel>>
    {
        public Controllers(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<ICleaner> GetCleaners()
        {
            yield return Cleaner<DirectoryCleaner>()
                .AppendTargetPath("Api/Generated");
            yield return Cleaner<DirectoryCleaner>()
                .AppendTargetPath("Controllers/Generated");
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            yield return Generator<LocalBaseApiController>()
                .AppendOutputPath($"Api/Generated/LocalBaseApiController.cs");

            foreach (var model in this.Model.Where(model => model.OnContext))
            {
                yield return Generator<ApiController>()
                    .WithModel(model)
                    .AppendOutputPath($"Api/Generated/{model.Name}ControllerGen.cs");

                yield return Generator<ViewController>()
                    .WithModel(model)
                    .AppendOutputPath($"Controllers/Generated/{model.Name}ControllerGen.cs");
            }
        }
    }
}

using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class Controllers : IntelliTect.Coalesce.CodeGeneration.Api.Generators.Controllers
    {
        public Controllers(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<ICleaner> GetCleaners()
        {
            foreach (var cleaner in base.GetCleaners()) yield return cleaner;
            
            yield return Cleaner<DirectoryCleaner>()
                .AppendTargetPath("Controllers/Generated");
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            foreach (var generator in base.GetGenerators()) yield return generator;

            foreach (var model in Model.CrudApiBackedClasses)
            {
                if (
#pragma warning disable CS0618 // Type or member is obsolete
                    (model.GetAttributeValue<CreateControllerAttribute, bool>(a => a.WillCreateView) ?? true)
#pragma warning restore CS0618 // Type or member is obsolete
                    && model.SecurityInfo.IsReadAllowed()
                )
                {
                    yield return Generator<ViewController>()
                        .WithModel(model)
                        .AppendOutputPath($"Controllers/Generated/{model.ControllerName}Controller.g.cs");
                }
            }
        }
    }
}

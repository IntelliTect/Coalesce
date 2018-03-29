using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
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
            foreach (var context in this.Model.DbContexts)
            {
                var entityLookup = context.Entities.ToLookup(e => e.ClassViewModel);

                var contextTypes = context.Entities
                    .Select(e => e.ClassViewModel)
                    .Union(Model.CustomDtos.Where(dto => 
                        entityLookup.Contains(dto.DtoBaseViewModel)
                    ));
                
                foreach (var model in contextTypes)
                {
                    if (model.WillCreateApiController)
                    {
                        yield return Generator<ModelApiController>()
                            .WithModel(model)
                            .WithDbContext(context.ClassViewModel)
                            .AppendOutputPath($"Api/Generated/{model.Name}Controller.g.cs");
                    }
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

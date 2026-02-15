using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators;

public class Models : CompositeGenerator<ReflectionRepository>
{
    public Models(CompositeGeneratorServices services) : base(services) { }

    public override IEnumerable<ICleaner> GetCleaners()
    {
        yield return Cleaner<DirectoryCleaner>()
            .AppendTargetPath("Generated");
    }

    public override IEnumerable<IGenerator> GetGenerators()
    {
        foreach (var model in this.Model.Entities.Union(Model.ExternalTypes))
        {
            yield return Generator<ClassDto>()
                .WithModel(model)
                .AppendOutputPath($"Generated/{model.ClientTypeName}Dto.g.cs");
            
        }
    }
}

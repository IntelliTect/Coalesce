using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators;

public class Scripts : CompositeGenerator<ReflectionRepository>
{
    public Scripts(CompositeGeneratorServices services) : base(services) { }

    public override IEnumerable<IGenerator> GetGenerators()
    {
        yield return Generator<TsMetadata>()
            .WithModel(Model)
            .AppendOutputPath("metadata.g.ts");

        yield return Generator<TsModels>()
            .WithModel(Model)
            .AppendOutputPath("models.g.ts");

        yield return Generator<TsViewModels>()
            .WithModel(Model)
            .AppendOutputPath("viewmodels.g.ts");

        yield return Generator<TsApiClients>()
            .WithModel(Model)
            .AppendOutputPath("api-clients.g.ts");

    }
}

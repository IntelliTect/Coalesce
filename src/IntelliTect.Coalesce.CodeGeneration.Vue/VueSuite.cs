using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators;

public class VueSuite : CompositeGenerator<ReflectionRepository>, IRootGenerator
{
    public VueSuite(CompositeGeneratorServices services) : base(services) { }

    public override IEnumerable<IGenerator> GetGenerators()
    {
        // Output to "src" is the default folder for vue-cli based projects,
        // which is the golden standard to support. Anything other than this
        // can still be changed with configuration.
        yield return Generator<Scripts>()
            .WithModel(Model)
            .AppendOutputPath("src");

        yield return Generator<Api.Generators.Models>()
            .WithModel(Model)
            .AppendOutputPath("Models");

        yield return Generator<Controllers>()
            .WithModel(Model);

        yield return Generator<KernelPlugins>()
            .WithModel(Model);
    }
}

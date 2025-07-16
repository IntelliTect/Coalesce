using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators;

public class KernelPlugins : CompositeGenerator<ReflectionRepository>
{
    public KernelPlugins(CompositeGeneratorServices services) : base(services) { }

    public override IEnumerable<ICleaner> GetCleaners()
    {
        yield return Cleaner<DirectoryCleaner>()
            .AppendTargetPath("KernelPlugins/Generated");
    }

    public override IEnumerable<IGenerator> GetGenerators()
    {
        foreach (var model in Model.CrudApiBackedClasses)
        {
            if (
                model.HasAttribute<KernelPluginAttribute>() ||
                model.ClientDataSources(Model).Any(ds => ds.HasAttribute<KernelPluginAttribute>()) ||
                model.KernelMethods.Any()
            )
            {
                yield return Generator<KernelPlugin>()
                    .WithModel(model)
                    .AppendOutputPath($"KernelPlugins/Generated/{model.ClientTypeName}KernelPlugin.g.cs");
            }
        }

        foreach (var model in Model.Services)
        {
            if (model.KernelMethods.Any())
            {
                yield return Generator<KernelPlugin>()
                    .WithModel(model)
                    .AppendOutputPath($"KernelPlugins/Generated/{model.ClientTypeName}KernelPlugin.g.cs");
            }
        }
    }
}

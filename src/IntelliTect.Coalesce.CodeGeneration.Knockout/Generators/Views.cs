using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class Views : CompositeGenerator<ReflectionRepository>
    {
        public Views(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<ICleaner> GetCleaners()
        {
            yield return Cleaner<DirectoryCleaner>()
                .AppendTargetPath("Generated")
                .WithDepth(SearchOption.AllDirectories);
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            yield return Generator<StaticApiViews>();

            foreach (var model in this.Model.ApiBackedClasses)
            {
                yield return Generator<TableView>()
                    .WithModel(model)
                    .AppendOutputPath($"Generated/{model.Name}/Table.cshtml");

                yield return Generator<CardView>()
                    .WithModel(model)
                    .AppendOutputPath($"Generated/{model.Name}/Cards.cshtml");

                yield return Generator<CreateEditView>()
                    .WithModel(model)
                    .AppendOutputPath($"Generated/{model.Name}/CreateEdit.cshtml");
            }
        }
    }
}

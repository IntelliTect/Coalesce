using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class Views : CompositeGenerator<List<ClassViewModel>>
    {
        public Views(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            foreach (var model in this.Model)
            {
                if (model.OnContext)
                {
                    yield return Generator<TableView>()
                        .WithModel(model)
                        .AppendOutputPath($"{model.Name}/Table.cshtml");
                    yield return Generator<CardView>()
                        .WithModel(model)
                        .AppendOutputPath($"{model.Name}/Cards.cshtml");
                    yield return Generator<CreateEditView>()
                        .WithModel(model)
                        .AppendOutputPath($"{model.Name}/CreateEdit.cshtml");
                }
            }
        }
    }
}

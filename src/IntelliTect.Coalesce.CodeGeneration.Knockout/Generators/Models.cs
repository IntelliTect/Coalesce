using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class Models : CompositeGenerator<List<ClassViewModel>>
    {
        public Models(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            foreach (var model in this.Model.Where(model => model.OnContext && !model.IsDto))
            {
                yield return Generator<ClassDto>()
                    .WithModel(model)
                    .AppendOutputPath($"Ko.{model.Name}.Gen.cs");
                
            }
        }
    }
}

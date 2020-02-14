using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class StaticApiViews : CompositeGenerator<object>
    {
        public StaticApiViews(CompositeGeneratorServices services) : base(services) { }
        
        
        private readonly string[] files =
        {
            //"Docs.cshtml", // defunct
            "EditorHtml.cshtml",
        };

        public override IEnumerable<IGenerator> GetGenerators()
        {

            foreach (var file in files)
            {
                yield return Generator<StaticFileGenerator>()
                    .WithTemplate(new TemplateDescriptor("Templates/Views/Api", file))
                    .AppendOutputPath("Generated/Api")
                    .AppendOutputPath(file);
            }
        }
    }
}

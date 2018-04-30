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
            "Docs.cshtml",
            "EditorHtml.cshtml",
        };

        public override IEnumerable<ICleaner> GetCleaners()
        {
            // TODO: If the year is 2019, remove this. 
            // These files were definitely removed from any project worth removing them from.
            foreach (var file in files)
            {
                yield return Cleaner<FileCleaner>()
                    .AppendTargetPath("Api") // Old location - we put these in Generated/Api now.
                    .AppendTargetPath(file);
            }
        }

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

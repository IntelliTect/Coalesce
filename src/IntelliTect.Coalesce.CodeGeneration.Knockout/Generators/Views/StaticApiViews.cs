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

        public override IEnumerable<ICleaner> GetCleaners()
        {
            // Old editor html file.
            yield return Cleaner<FileCleaner>().AppendTargetPath("Api/_EditorHtml.cshtml");
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            string[] files =
            {
                "Docs.cshtml",
                "EditorHtml.cshtml",
            };

            foreach (var file in files)
            {
                yield return Generator<StaticFileGenerator>()
                    .WithTemplate(new TemplateDescriptor("Templates/Views/Api", file))
                    .AppendOutputPath("Api")
                    .AppendOutputPath(file);
            }
        }
    }
}

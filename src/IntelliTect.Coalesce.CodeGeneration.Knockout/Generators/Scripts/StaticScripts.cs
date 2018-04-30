using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class StaticScripts : CompositeGenerator<object>
    {
        public StaticScripts(CompositeGeneratorServices services) : base(services) { }

        public override IEnumerable<ICleaner> GetCleaners()
        {
            yield return Cleaner<DirectoryCleaner>()
                .AppendTargetPath("Coalesce");
        }

        public override IEnumerable<IGenerator> GetGenerators()
        {
            string[] files =
            {
                    "coalesce.ko.base.ts",
                    "coalesce.ko.bindings.ts",
                    "coalesce.ko.utilities.ts",
                    "coalesce.utilities.ts",
            };

            foreach (var file in files)
            {
                yield return Generator<StaticFileGenerator>()
                    .WithTemplate(new TemplateDescriptor("Templates/Scripts/Coalesce", file))
                    .AppendOutputPath("Coalesce")
                    .AppendOutputPath(file);
            }

            var fileName = "coalesce.dependencies.d.ts";
            yield return Generator<StaticFileGenerator>()
                .WithTemplate(new TemplateDescriptor("Templates/Scripts", fileName))
                .AppendOutputPath(fileName)
                .PreventOverwrite();
        }
    }
}

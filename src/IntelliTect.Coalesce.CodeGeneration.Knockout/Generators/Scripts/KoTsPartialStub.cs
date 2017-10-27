using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoTsPartialStub : RazorTemplateGenerator<ClassViewModel>
    {
        public KoTsPartialStub(RazorTemplateServices razorServices) : base(razorServices) { }

        public override bool ShouldGenerate()
        {
            return !File.Exists(OutputPath);
        }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "KoViewModelPartial.cshtml");
    }
}

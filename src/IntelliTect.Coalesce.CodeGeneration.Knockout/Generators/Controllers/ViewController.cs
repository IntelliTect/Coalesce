using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class ViewController : RazorTemplateCSharpGenerator<ClassViewModel>
    {
        public ViewController(RazorTemplateServices razorServices) : base(razorServices) { }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "ViewController.cshtml");
    }
}

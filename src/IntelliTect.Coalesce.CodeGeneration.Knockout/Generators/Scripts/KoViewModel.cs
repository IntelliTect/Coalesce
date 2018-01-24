using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoViewModel : KnockoutVMGenerator
    {
        public KoViewModel(RazorTemplateServices razorServices) : base(razorServices)
        {
            MethodsLoadParent = true;
            ParentLoadHasIdParameter = true;
        }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "KoViewModel.cshtml");
    }
}

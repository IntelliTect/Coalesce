using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoViewModel : RazorTemplateGenerator<ClassViewModel>
    {
        public KoViewModel(ITemplateResolver resolver, RazorTemplateCompiler compiler) : base(resolver, compiler)
        {
        }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "KoViewModel.cshtml");
    }
}

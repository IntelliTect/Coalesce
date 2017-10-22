using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class CreateEditView : RazorTemplateGenerator<ClassViewModel>
    {
        public CreateEditView(RazorServices razorServices) : base(razorServices) { }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "CreateEditView.cshtml");
    }
}

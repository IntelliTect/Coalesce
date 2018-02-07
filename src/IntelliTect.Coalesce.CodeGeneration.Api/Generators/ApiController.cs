using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ApiController : RazorTemplateCSharpGenerator<ClassViewModel>
    {
        public ApiController(RazorTemplateServices razorServices) : base(razorServices) { }

        public ClassViewModel DbContext { get; set; }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "ApiController.cshtml");

        public ApiController WithDbContext(ClassViewModel contextViewModel)
        {
            DbContext = contextViewModel;
            return this;
        }
    }
}

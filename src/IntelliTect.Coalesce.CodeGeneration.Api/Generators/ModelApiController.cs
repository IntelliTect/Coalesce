using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ModelApiController : ApiController
    {
        public ModelApiController(RazorTemplateServices razorServices) : base(razorServices) { }

        public ClassViewModel DbContext { get; set; }

        public override TemplateDescriptor Template =>
            new TemplateDescriptor("Templates", "ModelApiController.cshtml");

        public ModelApiController WithDbContext(ClassViewModel contextViewModel)
        {
            DbContext = contextViewModel;
            return this;
        }
    }
}

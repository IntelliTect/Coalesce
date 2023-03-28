using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ServiceApiController : ApiController
    {
        public ServiceApiController(GeneratorServices services) : base(services) { }

        public override void BuildOutput(CSharpCodeBuilder b)
        {
            ClassViewModel model = Model;
            string namespaceName = WriteNamespaces(b);

            b.Line();
            using (b.Block($"namespace {namespaceName}.Api"))
            {
                WriteControllerRouteAttribute(b);
                /* No controller-level security annotation is applied - all security for service controllers is on a per-action basis. */
                b.Line("[ServiceFilter(typeof(IApiActionFilter))]");
                using (b.Block($"public partial class {Model.ApiControllerClassName} : Controller"))
                {
                    WriteClassContents(b);
                }
            }
        }

        private void WriteClassContents(CSharpCodeBuilder b)
        {
            b.Line($"protected ClassViewModel GeneratedForClassViewModel {{ get; }}");
            b.Line($"protected {Model.FullyQualifiedName} Service {{ get; }}");
            b.Line($"protected CrudContext Context {{ get; }}");
            b.Line();
            using (b.Block($"public {Model.ApiControllerClassName}(CrudContext context, {Model.FullyQualifiedName} service)"))
            {
                b.Line($"GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<{Model.FullyQualifiedName}>();");
                b.Line("Service = service;");
                b.Line("Context = context;");
            }

            foreach (var method in Model.ClientMethods)
            {
                WriteControllerActionPreamble(b, method);
                using (WriteControllerActionSignature(b, method))
                {
                    WriteMethodInvocation(b, method, "Service");

                    WriteMethodResultProcessBlock(b, method);
                }
            }
        }
    }
}

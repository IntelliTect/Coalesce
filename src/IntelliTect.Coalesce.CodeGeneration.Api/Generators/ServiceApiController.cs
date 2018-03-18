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
                if (Model.ApiRouted)
                {
                    if (!string.IsNullOrWhiteSpace(AreaName))
                    {
                        b.Line($"[Route(\"{AreaName}/api/{Model.ApiRouteControllerPart}\")]");
                    }
                    else
                    {
                        b.Line($"[Route(\"api/{Model.ApiRouteControllerPart}\")]");
                    }
                }
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
            b.Line($"protected {Model.FullyQualifiedName} Service {{ get; }}");
            using (b.Block($"public {Model.ApiControllerClassName}({Model.FullyQualifiedName} service)"))
            {
                b.Line("Service = service;");
            }

            b.Line();
            foreach (var method in Model.ClientMethods)
            {
                b.Line("/// <summary>");
                b.Line($"/// Method: {method.Name}");
                b.Line("/// </summary>");
                b.Line($"[{method.ApiActionHttpMethodAnnotation}(\"{method.Name}\")]");
                b.Line($"{method.SecurityInfo.ExecuteAnnotation}");
                using (b.Block($"{Model.ApiActionAccessModifier} virtual {method.ReturnTypeNameForApi} {method.Name} ({method.CsParameters})"))
                {
                    if (method.ResultType.HasClassViewModel ||
                       (method.ResultType.PureType.HasClassViewModel && method.ResultType.IsCollection))
                    {
                        b.Line("IncludeTree includeTree = null;");
                    }

                    if (method.ReturnType.IsVoid)
                    {
                        b.Line($"Service.{method.Name}({method.CsArguments});");
                    }
                    else
                    {
                        b.Line($"var methodResult = Service.{method.Name}({method.CsArguments});");
                    }

                    WriteMethodResultProcessBlock(b, method);
                }
            }
        }
    }
}

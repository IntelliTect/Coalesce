using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoServiceClient : KnockoutViewModelGenerator
    {
        public KoServiceClient(RazorTemplateServices razorServices) : base(razorServices) { }
        
        public override void BuildOutput(TypeScriptCodeBuilder b)
        {
            using (b.Block($"module {ModuleName(ModuleKind.Service)}"))
            {
                using (b.Block($"export class {Model.ServiceClientClassName}"))
                {
                    b.Line();
                    b.Line($"public readonly apiController: string = \"/{Model.ApiRouteControllerPart}\";");

                    b.Line();
                    b.Line($"public static coalesceConfig = new Coalesce.ServiceClientConfiguration<{Model.ServiceClientClassName}>(Coalesce.GlobalConfiguration.serviceClient);");

                    b.Line();
                    b.Line($"public coalesceConfig: Coalesce.ServiceClientConfiguration<{Model.ServiceClientClassName}>");
                    b.Indented($"= new Coalesce.ServiceClientConfiguration<{Model.ServiceClientClassName}>({Model.ServiceClientClassName}.coalesceConfig);");

                    b.Line();
                    foreach (var method in Model.ClientMethods)
                    {
                        WriteClientMethodDeclaration(b, method, Model.ServiceClientClassName);
                    }
                }
            }
        }
    }
}

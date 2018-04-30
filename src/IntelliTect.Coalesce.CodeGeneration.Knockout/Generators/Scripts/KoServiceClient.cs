using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoServiceClient : KnockoutViewModelGenerator
    {
        public KoServiceClient(GeneratorServices services) : base(services) { }
        
        public override void BuildOutput(TypeScriptCodeBuilder b)
        {
            using (b.Block($"module {ModuleName(ModuleKind.Service)}"))
            {
                using (b.Block($"export class {Model.ServiceClientClassName}"))
                {
                    b.Line();
                    b.Line($"public readonly apiController: string = \"/{Model.ApiRouteControllerPart}\";");

                    b.DocComment($"Configuration for all instances of {Model.ServiceClientClassName}. Can be overidden on each instance via instance.coalesceConfig.");
                    b.Line($"public static coalesceConfig = new Coalesce.ServiceClientConfiguration<{Model.ServiceClientClassName}>(Coalesce.GlobalConfiguration.serviceClient);");

                    b.DocComment($"Configuration for this {Model.ServiceClientClassName} instance.");
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

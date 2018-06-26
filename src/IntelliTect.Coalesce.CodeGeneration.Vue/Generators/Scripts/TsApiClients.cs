using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Vue.Utils;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class TsApiClients : StringBuilderFileGenerator<ReflectionRepository>
    {
        public TsApiClients(GeneratorServices services) : base(services)
        {
        }

        public override Task<string> BuildOutputAsync()
        {
            var b = new TypeScriptCodeBuilder(indentSize: 2);
            b.Lines(new [] {
                "import * as $metadata from './metadata.g'",
                "import * as $models from './models.g'",
                "import * as qs from 'qs'",
                "import { AxiosClient, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'",
                "import { AxiosResponse, AxiosRequestConfig } from 'axios'",
            });
            b.Line();

            foreach (var model in Model.Entities.OrderBy(e => e.ClientTypeName))
            {
                WriteApiClientClass(b, model, $"ModelApiClient<$models.{model.ClientTypeName}>");

                // Lines between classes
                b.Line();
                b.Line();
            }


            foreach (var model in Model.Services.OrderBy(e => e.ClientTypeName))
            {
                WriteApiClientClass(b, model, $"ServiceApiClient<typeof $metadata.{model.ClientTypeName}>");

                // Lines between classes
                b.Line();
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }

        private static void WriteApiClientClass(TypeScriptCodeBuilder b, ClassViewModel model, string baseClass)
        {
            string clientName = $"{model.ClientTypeName}ApiClient";
            using (b.Block($"export class {clientName} extends {baseClass}"))
            {
                b.Line($"constructor() {{ super($metadata.{model.ClientTypeName}) }}");

                foreach (var method in model.ClientMethods)
                {
                    WriteApiEndpointFunction(b, model, method);
                }
            }
        }

        private static void WriteApiEndpointFunction(TypeScriptCodeBuilder b, ClassViewModel model, MethodViewModel method)
        {
            var returnIsListResult = method.ReturnsListResult;
            string signature =
                string.Concat(method.ClientParameters.Select(f => $"{f.Name}: {new VueType(f.Type).TsType("$models")} | null, "))
                + "$config?: AxiosRequestConfig";

            if (method.IsModelInstanceMethod)
            {
                signature = $"id: {new VueType(model.PrimaryKey.Type).TsType(null)}, " + signature;
            }

            using (b.Block($"public {method.JsVariable}({signature})"))
            {
                string resultType = method.TransportTypeGenericParameter.IsVoid
                    ? $"{method.TransportType}<void>"
                    : $"{method.TransportType}<{new VueType(method.TransportTypeGenericParameter).TsType("$models")}>";

                b.Line($"const $method = this.$metadata.methods.{method.JsVariable}");
                using (b.Block($"const $params = this.$mapParams($method,", ')'))
                {
                    if (method.IsModelInstanceMethod)
                    {
                        b.Line($"id,");
                    }
                    foreach (var param in method.ClientParameters)
                    {
                        b.Line($"{param.JsVariable},");
                    }
                }

                b.Line("return AxiosClient");
                using (b.Indented())
                {
                    b.Line($".{method.ApiActionHttpMethodName.ToLower()}(");
                    b.Indented($"`/${{this.$metadata.controllerRoute}}/{method.Name}`,");
                    switch (method.ApiActionHttpMethod)
                    {
                        case DataAnnotations.HttpMethod.Get:
                        case DataAnnotations.HttpMethod.Delete:
                            b.Indented($"this.$options(undefined, $config, $params)");
                            break;
                        default:
                            b.Indented($"qs.stringify($params),");
                            b.Indented($"this.$options(undefined, $config)");
                            break;
                    }
                    b.Line(")");

                    b.Line($".then<AxiosResponse<{resultType}>>(r => this.$hydrate{method.TransportType}(r, $method.return))");
                }
            }

            // Line between methods
            b.Line();
        }
    }
}

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
                "import { AxiosPromise, AxiosResponse, AxiosRequestConfig } from 'axios'",
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
                string.Concat(method.ClientParameters.Select(f => $"{f.JsVariable}: {new VueType(f.Type).TsType("$models")} | null, "))
                + "$config?: AxiosRequestConfig";

            if (method.IsModelInstanceMethod)
            {
                signature = $"id: {new VueType(model.PrimaryKey.Type).TsType(null)}, " + signature;
            }

            string resultType = method.TransportTypeGenericParameter.IsVoid
                ? $"{method.TransportType}<void>"
                : $"{method.TransportType}<{new VueType(method.TransportTypeGenericParameter).TsType("$models")}>";

            using (b.Block($"public {method.JsVariable}({signature}): AxiosPromise<{resultType}>"))
            {
                b.Line($"const $method = this.$metadata.methods.{method.JsVariable}");
                using (b.Block($"const $params = "))
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

                b.Line("return this.$invoke($method, $params, $config)");
            }

            // Line between methods
            b.Line();
        }
    }
}

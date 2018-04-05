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
                "import * as $isValid from 'date-fns/isValid'",
                "import * as $format from 'date-fns/format'",
                "import { AxiosClient, ModelApiClient, ServiceApiClient, ItemResult, ListResult } from 'coalesce-vue/lib/api-client'",
                "import { AxiosResponse, AxiosRequestConfig } from 'axios'",
            });
            b.Line();

            foreach (var model in Model.Entities)
            {
                WriteApiClientClass(b, model, $"ModelApiClient<typeof $metadata.{model.ClientTypeName}, $models.{model.ClientTypeName}>");

                // Lines between classes
                b.Line();
                b.Line();
            }


            foreach (var model in Model.Services)
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

                using (b.Block($"const $params = this.$mapParams(this.$metadata.methods.{method.JsVariable},", ')'))
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
                    bool needsHydration = method.ResultType.PureType.HasClassViewModel;

                    // Include the generic param on the request if the result doesn't need to be hydrated.
                    // TODO: date return types need to be hydrated, but aren't here.
                    // Once we start generating method metadata, we can make a generic client-side hydration function
                    // that will accept the ApiResult object and the method's metadata and perform whatever actions are needed.
                    string requestGenericParam = !needsHydration ? $"<{resultType}>" : "";

                    b.Line($".{method.ApiActionHttpMethodName.ToLower()}{requestGenericParam}(");
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

                    // TODO: date return types aren't handled here.
                    // Need to finish writing the dto-to-model converter in model.ts,
                    // then bring that in here and use it.
                    if (needsHydration)
                    {
                        b.Line($".then<AxiosResponse<{resultType}>>(r => this.$hydrate{method.TransportType}(r, $metadata.{method.ResultType.PureType.ClassViewModel.ViewModelClassName}))");
                    }
                }
            }

            // Line between methods
            b.Line();
        }
    }
}

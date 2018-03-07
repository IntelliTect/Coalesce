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
            var b = new TypeScriptCodeBuilder();
            b.Line("import * as models from './models.g'");
            b.Line("import * as qs from 'qs'");
            b.Line("import { mapToDto as $mapToDto } from 'coalesce-vue/lib/model'");
            b.Line("import { AxiosClient, ApiClient } from 'coalesce-vue/lib/api-client'");
            b.Line("import { AxiosRequestConfig } from 'axios'");
            b.Line();

            foreach (var model in Model.Entities)
            {
                string clientName = $"{model.ViewModelClassName}ApiClient";
                using (b.Block($"export class {clientName} extends ApiClient<models.{model.ViewModelClassName}>"))
                { 
                    foreach (var method in model.ClientMethods)
                    {
                        var returnIsListResult = method.ReturnsListResult;
                        string signature =
                            string.Concat(method.ClientParameters.Select(f => $"{f.Name}: {new VueType(f.Type).TsTypePlain("models")} | null, "))
                            + "config?: AxiosRequestConfig";

                        if (method.IsModelInstanceMethod)
                        {
                            signature = $"id: {new VueType(model.PrimaryKey.Type).TsTypePlain(null)}, " + signature;
                        }

                        using (b.Block($"public {method.JsVariable}({signature})"))
                        {
                            string ConvertArugment(ParameterViewModel param)
                            {
                                string argument = param.JsVariable;
                                if (param.Type.HasClassViewModel)
                                {
                                    return $"$mapToDto({argument})";
                                }

                                if (param.Type.IsDate)
                                {
                                    string format = "YYYY-MM-DDTHH:mm:ss.SSS";
                                    if (param.Type.IsDateTimeOffset) format += "Z";
                                    
                                    return $"{argument} instanceof Date && isValid({argument}) ? {argument}.format('{format}') : null";
                                }
                                return argument;
                            }

                            var paramsObjectProps = method.ClientParameters
                                .Select(f => $"{f.JsVariable}: {ConvertArugment(f)}");

                            if (method.IsModelInstanceMethod)
                            {
                                paramsObjectProps = new[] { "id: id" }.Concat(paramsObjectProps);
                            }
                            var paramsObject = "{ " + string.Join(", ", paramsObjectProps) + " }";

                            b.Line($"const $params = {paramsObject}");
                            b.Line("return AxiosClient");
                            using (b.Block())
                            {
                                b.Line($".{method.ApiActionHttpMethodName.ToLower()}(");
                                b.Indented($"`/${{this.$metadata.controllerRoute}}/{method.Name}`,");
                                switch (method.ApiActionHttpMethod)
                                {
                                    case DataAnnotations.HttpMethod.Get:
                                    case DataAnnotations.HttpMethod.Delete:
                                        b.Indented($"this.$options(undefined, config, $params)");
                                        break;
                                    default:
                                        b.Indented($"qs.stringify($params),");
                                        b.Indented($"this.$options(undefined, config)");
                                        break;
                                }
                                b.Line(")");
                            }
                        }

                        // Line between methods
                        b.Line();
                    }
                }

                // Lines between classes
                b.Line();
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }
    }
}

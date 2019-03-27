using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Vue.Utils;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class TsViewModels : StringBuilderFileGenerator<ReflectionRepository>
    {
        public TsViewModels(GeneratorServices services) : base(services)
        {
        }

        public override Task<string> BuildOutputAsync()
        {
            var b = new TypeScriptCodeBuilder(indentSize: 2);

            b.Line("import * as $metadata from './metadata.g'");
            b.Line("import * as $models from './models.g'");
            b.Line("import * as $apiClients from './api-clients.g'");
            b.Line("import { ViewModel, ListViewModel, defineProps } from 'coalesce-vue/lib/viewmodel'");
            b.Line();

            foreach (var model in Model.Entities)
            {
                WriteViewModel(b, model);
                WriteListViewModel(b, model);
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }

        private static void WriteViewModel(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            string name = model.ViewModelClassName;
            string viewModelName = $"{name}ViewModel";
            string metadataName = $"$metadata.{name}";

            b.Line($"export interface {viewModelName} extends $models.{name} {{}}");
            using (b.Block($"export class {viewModelName} extends ViewModel<$models.{name}, $apiClients.{name}ApiClient>"))
            {
                // This is an alternative to calling defineProps() on each class that causes larger and more cluttered files:
                //foreach (var prop in model.ClientProperties)
                //{
                //    b.Line($"get {prop.JsVariable}() {{ return this.$data.{prop.JsVariable} }}");
                //    b.Line($"set {prop.JsVariable}(val) {{ this.$data.{prop.JsVariable} = val }}");
                //}

                foreach (var method in model.ClientMethods.Where(m => !m.IsStatic))
                {
                    string signature =
                        string.Concat(method.ClientParameters.Select(f => $", {f.Name}: {new VueType(f.Type).TsType("$models")} | null"));

                    // "item" or "list"
                    var transportTypeSlug = method.TransportType.ToString().Replace("Result", "").ToLower();

                    b.DocComment(method.Comment, true);
                    b.Line($"public {method.JsVariable} = this.$apiClient.$makeCaller(\"{transportTypeSlug}\", ");
                    b.Indented($"(c{signature}) => c.{method.JsVariable}(this.$primaryKey{string.Concat(method.ClientParameters.Select(p => ", " + p.Name))}))");
                }

                b.Line();
                using (b.Block($"constructor(initialData?: $models.{name})"))
                {
                    b.Line($"super({metadataName}, new $apiClients.{name}ApiClient(), initialData)");
                }
            }
            b.Line($"defineProps({viewModelName}, $metadata.{name})");
            b.Line();
        }

        private static void WriteListViewModel(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            string name = model.ViewModelClassName;

            string viewModelName = $"{model.ListViewModelClassName}ViewModel";
            string metadataName = $"$metadata.{name}";

            using (b.Block($"export class {viewModelName} extends ListViewModel<$models.{name}, $apiClients.{name}ApiClient>"))
            {

                foreach (var method in model.ClientMethods.Where(m => m.IsStatic))
                {
                    string signature =
                        string.Concat(method.ClientParameters.Select(f => $", {f.Name}: {new VueType(f.Type).TsType("$models")} | null"));

                    // "item" or "list"
                    var transportTypeSlug = method.TransportType.ToString().Replace("Result", "").ToLower();

                    b.DocComment(method.Comment, true);
                    b.Line($"public {method.JsVariable} = this.$apiClient.$makeCaller(\"{transportTypeSlug}\", ");
                    b.Indented($"(c{signature}) => c.{method.JsVariable}({string.Join(", ", method.ClientParameters.Select(p => p.Name))}))");
                }

                b.Line();
                using (b.Block($"constructor()"))
                {
                    b.Line($"super({metadataName}, new $apiClients.{name}ApiClient())");
                }
            }
            b.Line();
        }
    }
}

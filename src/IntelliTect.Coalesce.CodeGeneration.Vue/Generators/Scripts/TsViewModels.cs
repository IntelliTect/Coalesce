using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Vue.Utils;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
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
            b.Line("import { ViewModel, ListViewModel, ServiceViewModel, DeepPartial, defineProps } from 'coalesce-vue/lib/viewmodel'");
            b.Line();

            foreach (var model in Model.ApiBackedClasses.OrderBy(e => e.ClientTypeName))
            {
                WriteViewModel(b, model);
                WriteListViewModel(b, model);
                b.Line();
            }

            foreach (var model in Model.Services.OrderBy(e => e.ClientTypeName))
            {
                WriteServiceViewModel(b, model);
                b.Line();
            }

            using (b.Block("const viewModelTypeLookup = ViewModel.typeLookup ="))
            {
                foreach (var model in Model.ApiBackedClasses.OrderBy(e => e.ClientTypeName))
                {
                    b.Line($"{model.ViewModelClassName}: {model.ViewModelClassName}ViewModel,");
                }
            }
            using (b.Block("const listViewModelTypeLookup = ListViewModel.typeLookup ="))
            {
                foreach (var model in Model.ApiBackedClasses.OrderBy(e => e.ClientTypeName))
                {
                    b.Line($"{model.ViewModelClassName}: {model.ListViewModelClassName}ViewModel,");
                }
            }
            using (b.Block("const serviceViewModelTypeLookup = ServiceViewModel.typeLookup ="))
            {
                foreach (var model in Model.Services.OrderBy(e => e.ClientTypeName))
                {
                    b.Line($"{model.ViewModelClassName}: {model.ViewModelClassName}ViewModel,");
                }
            }

            b.Line();

            return Task.FromResult(b.ToString());
        }

        private static void WriteViewModel(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            string name = model.ViewModelClassName;
            string viewModelName = $"{name}ViewModel";
            string metadataName = $"$metadata.{name}";

            using (b.Block($"export interface {viewModelName} extends $models.{name}"))
            {
                foreach (var prop in model.ClientProperties)
                {
                    b.DocComment(prop.Comment);
                    var typeString = new VueType(prop.Type.NullableUnderlyingType).TsType(modelPrefix: "$models", viewModel: true);
                    b.Line($"{prop.JsVariable}: {typeString} | null;");
                }
            }

            using (b.Block($"export class {viewModelName} extends ViewModel<$models.{name}, $apiClients.{name}ApiClient, {model.PrimaryKey.Type.TsType}> implements $models.{name} "))
            {
                foreach (var prop in model.ClientProperties)
                {

                    // Eventually, this should support any collection of models.
                    // For now, though, `this.$addChild` only supports collection navigations.
                    // if (prop.Type.TsTypeKind == TypeDiscriminator.Collection && prop.PureType.TsTypeKind == TypeDiscriminator.Model)

                    if (prop.Role == PropertyRole.CollectionNavigation)
                    {
                        b.Line();

                        var vt = new VueType(prop.PureType);
                        string propVmName = vt.TsType(viewModel: true);

                        b.Line();
                        using (b.Block($"public addTo{prop.Name}(): {propVmName}"))
                        {
                            b.Line($"return this.$addChild('{prop.JsVariable}')");
                        }

                        if (prop.IsManytoManyCollection)
                        {
                            b.Line();
                            var manyToManyType = new VueType(prop.ManyToManyCollectionProperty.Type).TsType(viewModel: true);
                            using (b.Block($"get {prop.ManyToManyCollectionName.ToCamelCase()}(): ReadonlyArray<{manyToManyType}>"))
                            {
                                b.Line($"return (this.{prop.JsVariable} || []).map($ => $.{prop.ManyToManyCollectionProperty.JsVariable}!).filter($ => $)");
                            }
                        }
                    }
                }

                foreach (var method in model.ClientMethods.Where(m => !m.IsStatic))
                {
                    WriteMethodCaller(b, method);
                }

                b.Line();
                using (b.Block($"constructor(initialData?: DeepPartial<$models.{name}> | null)"))
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

            using (b.Block($"export class {viewModelName} extends ListViewModel<$models.{name}, $apiClients.{name}ApiClient, {name}ViewModel>"))
            {

                foreach (var method in model.ClientMethods.Where(m => m.IsStatic))
                {
                    WriteMethodCaller(b, method);
                }

                b.Line();
                using (b.Block($"constructor()"))
                {
                    b.Line($"super({metadataName}, new $apiClients.{name}ApiClient())");
                }
            }
            b.Line();
        }

        private static void WriteServiceViewModel(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            string name = model.ViewModelClassName;

            string viewModelName = $"{name}ViewModel";
            string metadataName = $"$metadata.{name}";

            using (b.Block($"export class {viewModelName} extends ServiceViewModel<typeof {metadataName}, $apiClients.{name}ApiClient>"))
            {
                foreach (var method in model.ClientMethods)
                {
                    WriteMethodCaller(b, method);
                }

                b.Line();
                using (b.Block($"constructor()"))
                {
                    b.Line($"super({metadataName}, new $apiClients.{name}ApiClient())");
                }
            }
            b.Line();
        }


        private static void WriteMethodCaller(TypeScriptCodeBuilder b, MethodViewModel method)
        {
            string signature =
                        string.Concat(method.ClientParameters.Select(f => $", {f.Name}: {new VueType(f.Type).TsType("$models")} | null"));

            string argsConstructor =
                "({" +
                string.Concat(method.ClientParameters.Select(f => $"{f.Name}: null as {new VueType(f.Type).TsType("$models")} | null, ")) +
                "})";

            string pkArg = method.IsModelInstanceMethod ? "this.$primaryKey, " : "";
            
            var transportTypeSlug = method.TransportType.ToString().Replace("Result", "").ToLower();

            b.DocComment(method.Comment, true);
            using (b.Block($"public get {method.JsVariable}()"))
            {
                b.Line($"const {method.JsVariable} = this.$apiClient.$makeCaller(");
                // "item" or "list"
                b.Indented($"\"{transportTypeSlug}\", ");
                // The invoker function when the caller is used directly like `caller(...)`, or via `caller.invoke(...)`
                b.Indented($"(c{signature}) => c.{method.JsVariable}({pkArg}{string.Join(", ", method.ClientParameters.Select(p => p.Name))}),");
                // The factory function to return a new args object. Args object lives on `caller.args`
                b.Indented($"() => {argsConstructor},");
                // The invoker function when the caller is invoked with args with `caller.invokeWithArgs(args?)`
                b.Indented($"(c, args) => c.{method.JsVariable}({pkArg}{string.Join(", ", method.ClientParameters.Select(p => "args." + p.Name))}))");

                // Lazy getter technique - don't create the caller until/unless it is needed,
                // since creation of api callers is a little expensive.
                b.Line();
                b.Line($"Object.defineProperty(this, '{method.JsVariable}', {{value: {method.JsVariable}}});");
                b.Line($"return {method.JsVariable}");
            }
        }
    }
}

using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Vue.Utils;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Utilities;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
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
            b.Line("import { ViewModel, ListViewModel, ViewModelCollection, ServiceViewModel, type DeepPartial, defineProps, createAbstractProxyViewModelType } from 'coalesce-vue/lib/viewmodel'");
            b.Line();

            foreach (var model in Model.CrudApiBackedClasses.OrderBy(e => e.ClientTypeName))
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
                foreach (var model in Model.CrudApiBackedClasses.Where(e => !e.Type.IsAbstract).OrderBy(e => e.ClientTypeName))
                {
                    b.Line($"{model.ViewModelClassName}: {model.ViewModelClassName}ViewModel,");
                }
            }
            using (b.Block("const listViewModelTypeLookup = ListViewModel.typeLookup ="))
            {
                foreach (var model in Model.CrudApiBackedClasses.OrderBy(e => e.ClientTypeName))
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

        private void WriteViewModel(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            string name = model.ViewModelClassName;
            string modelName = new VueType(model.Type).TsType(modelPrefix: "$models");
            string viewModelName = new VueType(model.Type).TsType(viewModel: true);
            string metadataName = $"$metadata.{name}";

            if (model.Type.IsAbstract)
            {
                b.Line($"export type {viewModelName} = {string.Join(" | ", model.ClientDerivedTypes.Select(t => new VueType(t.Type).TsType(viewModel: true)))}");
                b.Line($"export const {viewModelName} = createAbstractProxyViewModelType<{modelName}, {viewModelName}>({metadataName}, $apiClients.{name}ApiClient)");
                b.Line();
                return;
            }

            using (b.Block($"export interface {viewModelName} extends {modelName}"))
            {
                foreach (var prop in model.ClientProperties)
                {
                    b.DocComment(prop.Comment ?? prop.Description);
                    var vueType = new VueType(prop.Type.NullableValueUnderlyingType);
                    var typeString = vueType.TsType(modelPrefix: "$models", viewModel: true);
                    var modelTypeString = vueType.TsType(modelPrefix: "$models", viewModel: false);

                    if (typeString == modelTypeString)
                    {
                        b.Line($"{prop.JsVariable}: {typeString} | null;");
                    }
                    else if (prop.Type.IsCollection)
                    {
                        var pureType = new VueType(prop.PureType);
                        var pureTypeString = pureType.TsType(modelPrefix: "$models", viewModel: true);
                        var pureModelTypeString = pureType.TsType(modelPrefix: "$models", viewModel: false);

                        b.Line($"get {prop.JsVariable}(): ViewModelCollection<{pureTypeString}, {pureModelTypeString}>;");
                        b.Line($"set {prop.JsVariable}(value: ({pureTypeString} | {pureModelTypeString})[] | null);");
                    }
                    else
                    {
                        b.Line($"get {prop.JsVariable}(): {typeString} | null;");
                        b.Line($"set {prop.JsVariable}(value: {typeString} | {modelTypeString} | null);");
                    }
                }
            }

            using (b.Block($"export class {viewModelName} extends ViewModel<{modelName}, $apiClients.{name}ApiClient, {new VueType(model.PrimaryKey.Type).TsType(modelPrefix: "$models")}> implements {modelName} "))
            {
                if (model.ClientDataSources(Model).Any())
                {
                    b.Line($"static DataSources = {modelName}.DataSources;");
                }

                WriteConsts(b, model);

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
                        using (b.Block($"public addTo{prop.Name}(initialData?: DeepPartial<$models.{vt.TsType()}> | null)"))
                        {
                            b.Line($"return this.$addChild('{prop.JsVariable}', initialData) as {propVmName}");
                        }

                        if (prop.IsManytoManyCollection)
                        {
                            b.Line();
                            var manyToManyType = new VueType(prop.ManyToManyFarNavigationProperty.Type).TsType(viewModel: true);
                            using (b.Block($"get {prop.ManyToManyCollectionName.ToCamelCase()}(): ReadonlyArray<{manyToManyType}>"))
                            {
                                b.Line($"return (this.{prop.JsVariable} || []).map($ => $.{prop.ManyToManyFarNavigationProperty.JsVariable}!).filter($ => $)");
                            }
                        }
                    }
                }

                foreach (var method in model.ClientMethods.Where(m => !m.IsStatic))
                {
                    WriteMethodCaller(b, method);
                }

                b.Line();
                using (b.Block($"constructor(initialData?: DeepPartial<{modelName}> | null)"))
                {
                    b.Line($"super({metadataName}, new $apiClients.{name}ApiClient(), initialData)");
                    if (model.IsCustomDto)
                    {
                        // Non-generated DTOs don't have the necessary guts for surgical saves to work.
                        b.Line("this.$saveMode = \"whole\"");
                    }
                }
            }
            b.Line($"defineProps({viewModelName}, $metadata.{name})");
            b.Line();
        }

        private void WriteListViewModel(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            string name = model.ViewModelClassName;

            string modelName = new VueType(model.Type).TsType(modelPrefix: "$models");
            string viewModelName = new VueType(model.Type).TsType(viewModel: true);
            string listViewModelName = $"{model.ListViewModelClassName}ViewModel";
            string metadataName = $"$metadata.{name}";

            using (b.Block($"export class {listViewModelName} extends ListViewModel<{modelName}, $apiClients.{name}ApiClient, {viewModelName}>"))
            {
                if (model.ClientDataSources(Model).Any())
                {
                    b.Line($"static DataSources = {modelName}.DataSources;");
                }

                WriteConsts(b, model);

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
                WriteConsts(b, model);

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
            var inputParams = method.ApiParameters
                .Where(p => p.ParentSourceProp is null);

            var paramTypeFlags = VueType.Flags.None;
            if (method.HasHttpRequestBody) paramTypeFlags |= VueType.Flags.RawBinary;

            var signatureData = inputParams.SignatureData(paramTypeFlags).ToList();

            string argsConstructor =
                "({" +
                string.Concat(inputParams.Select(f => $"{f.JsVariable}: null as {new VueType(f.Type, paramTypeFlags).TsType("$models")} | null, ")) +
                "})";

            var transportTypeSlug = method.TransportType.ToString().Replace("Result", "").ToLower();

            b.DocComment(method.Comment ?? method.Description, true);
            using (b.Block($"public get {method.JsVariable}()"))
            {
                b.Line($"const {method.JsVariable} = this.$apiClient.$makeCaller(");
                // The metadata of the method
                b.Indented($"this.$metadata.methods.{method.JsVariable},");

                // The invoker function when the caller is used directly like `caller(...)`, or via `caller.invoke(...)`
                string signature = string.Join(", ", signatureData.Select(p => p.Signature).Prepend("c"));
                string positionalParams = string.Join(", ", method.ApiParameters.Select(p => PropValue(p, "")));
                b.Indented($"({signature}) => c.{method.JsVariable}({positionalParams}),");

                // The factory function to return a new args object. Args object lives on `caller.args`
                b.Indented($"() => {argsConstructor},");

                // The invoker function when the caller is invoked with args with `caller.invokeWithArgs(args?)`
                var argsParams = string.Join(", ", method.ApiParameters.Select(p =>
                    PropValue(p, "args.")

                // NB: The following code is from an iteration of optional param handling
                // where required params did not emit `null` as part of their signature.
                // This design was scrapped because it just leads to slapping null forgiveness operators
                // all over callsites of methods when method parameters are coming from ViewModel props
                // or from bound `ref`s on a component.
                //// We have to suppress nullability issues when passing the args to the API client func
                //// since the args are always nullable. Perhaps in the future we could generate null checks
                //// and throw errors for missing required args?
                //+ (signatureData.FirstOrDefault(d => d.Param.Name == p.Name).IsRequired ? "!" : "")


                ));
                b.Indented($"(c, args) => c.{method.JsVariable}({argsParams}))");

                // Lazy getter technique - don't create the caller until/unless it is needed,
                // since creation of api callers is a little expensive.
                b.Line();
                b.Line($"Object.defineProperty(this, '{method.JsVariable}', {{value: {method.JsVariable}}});");
                b.Line($"return {method.JsVariable}");

                static string PropValue(ParameterViewModel p, string prefix)
                {
                    if (p.ParentSourceProp is { } src)
                    {
                        if (src.IsPrimaryKey)
                        {
                            return "this.$primaryKey";
                        }
                        return $"this.{src.JsVariable}";
                    }

                    if (prefix == "") return p.JsVariable.GetValidJsIdentifier();

                    return prefix + p.JsVariable;
                }
            }
        }

        static internal void WriteConsts(TypeScriptCodeBuilder b, ClassViewModel vm)
        {
            if (vm.ClientConsts.Any()) b.Line();

            foreach (var value in vm.ClientConsts)
            {
                b.Line($"static {value.Name.ToCamelCase()} = {value.ValueLiteralForTypeScript("$models.")}");
            }
        }
    }
}

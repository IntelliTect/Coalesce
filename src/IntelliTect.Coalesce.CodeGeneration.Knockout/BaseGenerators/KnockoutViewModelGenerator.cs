﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators
{
    public abstract class KnockoutViewModelGenerator : KnockoutTsGenerator
    {
        public KnockoutViewModelGenerator(GeneratorServices services) : base(services) { }
        
        public string ModuleName(ModuleKind kind)
        {
            string moduleName = kind.ToString() + "s";
            if (!string.IsNullOrWhiteSpace(AreaName))
                moduleName = AreaName + "." + moduleName;
            if (!string.IsNullOrWhiteSpace(ModulePrefix))
                moduleName = ModulePrefix + "." + moduleName;
            return moduleName;
        }

        public string ListViewModelModuleName => ModuleName(ModuleKind.ListViewModel);

        public string ViewModelModuleName => ModuleName(ModuleKind.ViewModel);

        public string ViewModelFullName => $"{ViewModelModuleName}.{Model.ViewModelClassName}";

        /// <summary>
        /// Writes the class for invoking the given client method,
        /// as well as the property for the default instance of this class.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="method"></param>
        /// <param name="parentClassName"></param>
        /// <param name="methodLoadsParent">
        /// If true, code emitted for methods that load their parents' type should
        /// load the method's parent with the results of the method when called.
        /// </param>
        /// <param name="parentLoadHasIdParameter">
        /// If true, calls to this.parent.load() will pass null as the first arg and a callback as the second.
        /// </param>
        public void WriteClientMethodDeclaration(
            TypeScriptCodeBuilder b, MethodViewModel method, string parentClassName,
            bool methodLoadsParent = false, bool parentLoadHasIdParameter = false
        )
        {
            var model = this.Model;
            var isService = method.Parent.IsService;
            var returnIsListResult = method.ReturnsListResult;

            string reloadArg = !isService ? ", reload" : "";
            string reloadParam = !isService ? ", reload: boolean = true" : "";
            string callbackAndReloadParam = $"callback?: (result: {method.ResultType.TsType}) => void{reloadParam}";

            // Default instance of the method class.
            b.DocComment($"Methods and properties for invoking server method {method.NameWithoutAsync}.\n\n{method.Comment}");
            b.Line($"public readonly {method.JsVariable} = new {parentClassName}.{method.NameWithoutAsync}(this);");

            string methodBaseClass = returnIsListResult
                ? "ClientListMethod"
                : "ClientMethod";

            // Not wrapping this in a using since it is used by nearly this entire method. Will manually dispose.
            var classBlock = b.Block(
                $"public static {method.NameWithoutAsync} = class {method.NameWithoutAsync} extends Coalesce.{methodBaseClass}<{parentClassName}, {method.ResultType.TsType}>", ';');

            b.Line($"public readonly name = '{method.NameWithoutAsync}';");
            b.Line($"public readonly verb = '{method.ApiActionHttpMethod.ToString().ToUpperInvariant()}';");

            if (method.ResultType.IsFile)
            {
                b.Line($"protected readonly returnsFile = true;");
            }

            if (method.ResultType.IsCollection)
            {
                b.Line($"public result: {method.ResultType.TsKnockoutType()} = {method.ResultType.ObservableConstructorCall()};");
            }

            // ----------------------
            // Standard invoke method - all CS method parameters as TS method parameters.
            // ----------------------
            b.Line();
            b.Line($"/** Calls server method ({method.NameWithoutAsync}) with the given arguments */");

            string parameters = "";
            parameters = string.Join(", ", method.ClientParameters.Select(f => $"{f.Name}: {f.Type.TsType} | null"));
            if (!string.IsNullOrWhiteSpace(parameters)) parameters += ", ";
            parameters += callbackAndReloadParam;

            using (b.Block($"public invoke = ({parameters}): JQueryPromise<any> =>", ';'))
            {
                string jsPostObject = "{ ";
                jsPostObject += string.Join(", ", method.ApiParameters.Select(f => $"{f.JsVariable}: {GetMethodParameterMapping(f)}"));
                jsPostObject += " }";

                b.Line($"return this.invokeWithData({jsPostObject}, callback{reloadArg});");
            }


            // ----------------------
            // Members for methods with parameters only.
            if (method.ClientParameters.Any())
            {
                b.Line();


                // ----------------------
                // Args class, and default instance
                b.Line($"/** Object that can be easily bound to fields to allow data entry for the method's parameters */");
                b.Line($"public args = new {method.NameWithoutAsync}.Args(); ");

                using (b.Block("public static Args = class Args", ';'))
                {
                    foreach (var arg in method.ClientParameters)
                    {
                        b.Line($@"public {arg.JsVariable}: {arg.Type.TsKnockoutType(true)} = {arg.Type.ObservableConstructorCall()};");
                    }
                }

                

                // Gets the js arguments to pass to a call to this.invoke(...)
                string JsArguments(string obj = "")
                {
                    string result;
                    if (obj != "")
                    {
                        result = string.Join(", ", method.ClientParameters.Select(f => $"{obj}.{f.JsVariable}()"));
                    }
                    else
                    {
                        result = string.Join(", ", method.ClientParameters.Select(f => obj + f.JsVariable));
                    }

                    if (!string.IsNullOrEmpty(result))
                    {
                        result += ", ";
                    }
                    result += "callback";

                    return result;
                }


                // ----------------------
                // invokeWithArgs method
                // ----------------------
                b.Line();
                b.Line($"/** Calls server method ({method.NameWithoutAsync}) with an instance of {method.NameWithoutAsync}.Args, or the value of this.args if not specified. */");
                // We can't explicitly declare the type of the args parameter here - TypeScript doesn't allow it.
                // Thankfully, we can implicitly type using the default.
                using (b.Block($"public invokeWithArgs = (args = this.args, {callbackAndReloadParam}): JQueryPromise<any> =>"))
                {
                    b.Line($"return this.invoke({JsArguments("args")}{reloadArg});");
                }


                // ----------------------
                // invokeWithPrompts method
                // ----------------------
                b.Line();
                b.Line("/** Invokes the method after displaying a browser-native prompt for each argument. */");
                using (b.Block($"public invokeWithPrompts = ({callbackAndReloadParam}): JQueryPromise<any> | undefined =>", ';'))
                {
                    b.Line($"var $promptVal: string | null = null;");
                    foreach (var param in method.ClientParameters.Where(f => f.ConvertsFromJsString))
                    {
                        b.Line($"$promptVal = {$"prompt('{param.Name.ToProperCase()}')"};");
                        // AES 1/23/2018 - why do we do this? what about optional params where no value is desired?
                        b.Line($"if ($promptVal === null) return;");
                        b.Line($"var {param.Name}: {param.Type.TsType} = {param.Type.TsConvertFromString("$promptVal")};");
                    }

                    // For all parameters that can't convert from a string (is this even possible with what we support for method parameters now?),
                    // pass null as the value. I guess we just let happen what's going to happen? Again, not sure when this case would ever be hit.
                    foreach (var param in method.ClientParameters.Where(f => !f.ConvertsFromJsString))
                    {
                        b.Line($"var {param.Name}: null = null;");
                    }
                    b.Line($"return this.invoke({JsArguments("")}{reloadArg});");
                }
            }



            // ----------------------
            // Method response handler - highly dependent on what the response type actually is.
            // ----------------------
            b.Line();

            var loadResponseType = 
                method.ResultType.IsFile ? "Blob" :
                $"Coalesce.{(returnIsListResult ? "List" : "Item")}Result";

            using (b.Block($"protected loadResponse = (data: {loadResponseType}, jqXHR: JQuery.jqXHR, {callbackAndReloadParam}) =>", ';'))
            {
                var incomingMainData = returnIsListResult
                    ? "data.list || []"
                    : "data.object";

                if (method.ResultType.IsCollection && method.ResultType.PureType.HasClassViewModel)
                {
                    // Collection of objects that have TypeScript ViewModels. This could be an entity or an external type.

                    // If the models have a key, rebuild our collection using that key so that we can reuse objects when the data matches.
                    var keyNameArg = method.ResultType.PureType.ClassViewModel.PrimaryKey != null
                        ? $"'{method.ResultType.PureType.ClassViewModel.PrimaryKey.JsVariable}'"
                        : "null";
                    
                     b.Line($"Coalesce.KnockoutUtilities.RebuildArray(this.result, {incomingMainData}, {keyNameArg}, ViewModels.{method.ResultType.PureType.ClassViewModel.ClientTypeName}, this, true);");
                }
                else if (method.ResultType.HasClassViewModel)
                {
                    // Single view model return type.

                    b.Line("if (!this.result()) {");
                    b.Indented($"this.result(new ViewModels.{method.ResultType.PureType.ClassViewModel.ClientTypeName}({incomingMainData}));");
                    b.Line("} else {");
                    b.Indented($"this.result().loadFromDto({incomingMainData});");
                    b.Line("}");
                }
                else if (method.ResultType.IsFile)
                {
                    b.Line("const file = new File([data], Coalesce.Utilities.getFileNameFromContentDisposition(jqXHR.getResponseHeader('content-disposition')), { type: data.type });");
                    b.Line("this.result(file);");
                    b.Line("if (this.resultObjectUrl()) URL.revokeObjectURL(this.resultObjectUrl()!);");
                    b.Line("this.resultObjectUrl(URL.createObjectURL(file));");
                }
                else
                {
                    // Uninteresting return type. Either void, a primitive, or a collection of primitives.
                    // In any case, regardless of the type of the 'result' observable, this is how we set the results.
                    b.Line($"this.result({incomingMainData});");
                }

                if (isService)
                {
                    b.Line("if (typeof(callback) == 'function')");
                    b.Indented("callback(this.result());");
                }
                else if (method.ResultType == method.Parent.Type && methodLoadsParent)
                {
                    // The return type is the type of the method's parent. Load the parent with the results of the method.
                    // Parameter 'reload' has no meaning here, since we're reloading the object with the result of the method.
                    b.Line($"this.parent.loadFromDto({incomingMainData}, true)");
                    using (b.Block("if (typeof(callback) == 'function')"))
                    {
                        b.Line($"callback(this.result());");
                    }
                }
                else
                {
                    // We're not loading the results into the method's parent. This is by far the more common case.
                    b.Line("if (reload) {");
                    // If reload is true, invoke the load function on the method's parent to reload its latest state from the server.
                    // Only after that's done do we call the method-completion callback.
                    // Store the result locally in case it somehow gets changed by us reloading the parent.
                    b.Indented("var result = this.result();");
                    b.Indented($"this.parent.load({(parentLoadHasIdParameter ? "null, " : "")}typeof(callback) == 'function' ? () => callback(result) : undefined);");
                    b.Line("} else if (typeof(callback) == 'function') {");
                    // If we're not reloading, just call the callback now.
                    b.Indented("callback(this.result());");
                    b.Line("}");
                }
            }

            if (method.ResultType.IsFile)
            {
                b.DocComment($"An object URL (https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL) representing the last successful response file.");
                b.Line("public resultObjectUrl: KnockoutObservable<string | null> = ko.observable(null);");
            }


            if (method.ApiActionHttpMethod == DataAnnotations.HttpMethod.Get)
            {
                b.DocComment($"URL for method '{method.NameWithoutAsync}'");
                b.Line($"public url: KnockoutComputed<string> = ko.pureComputed(() => ");
                var fileUrl = $"this.parent.coalesceConfig.baseApiUrl() + this.parent.apiController + '/' + this.name + '?'";

                string jsPostObject = "{ ";
                jsPostObject += string.Join(", ", method.ApiParameters.Select(f => $"{f.JsVariable}: {GetMethodParameterMapping(f, "this.args.")}"));
                jsPostObject += " }";

                b.Indented(fileUrl);
                b.Indented($"+ $.param({jsPostObject})");
                b.Line(");");
            }


            // End of the method class declaration.
            classBlock.Dispose();

        }

        private string GetMethodParameterMapping(ParameterViewModel param, string prefix = "")
        {
            string argument = prefix + param.JsVariable;
            if (param.ParentSourceProp != null)
            {
                argument = $"this.parent.{param.ParentSourceProp.JsVariable}()";
            }
            if (param.Type.IsCollection && param.Type.PureType.HasClassViewModel)
                return $"{argument}?.map($ => $.saveToDto())";
            if (param.Type.HasClassViewModel)
                return $"{argument}?.saveToDto()";
            if (param.Type.IsDate)
                return $"{argument}?.format({MomentDateDtoFormat(param.Type)})";
            return argument;
        }

        protected string MomentDateDtoFormat(TypeViewModel type) => $"'YYYY-MM-DDTHH:mm:ss.SSS{(type.IsDateTimeOffset ? "ZZ" : "")}'";

        public void WriteMethod_SaveToDto(TypeScriptCodeBuilder b)
        {
            b.DocComment($"Saves this object into a data transfer object to send to the server.");
            using var _ = b.Block($"public saveToDto = (): any =>");

            b.Line("var dto: any = {};");
            if (Model.PrimaryKey != null)
            {
                b.Line($"dto.{Model.PrimaryKey.JsonName} = this.{Model.PrimaryKey.JsVariable}();");
            }

            b.Line();

            // Objects are skipped in the loop because objects will be examined
            // at the same time that their corresponding foreign key is examined.
            // The PrimaryKey is skipped because it is handled first (above).
            foreach (PropertyViewModel prop in Model.ClientProperties.Where(f =>
                f.IsClientSerializable && f != Model.PrimaryKey
            ))
            {
                if (prop.Type.IsDate)
                {
                    b.Line($"if (!this.{prop.JsVariable}()) dto.{prop.JsonName} = null;");
                    b.Line($"else dto.{prop.JsonName} = this.{prop.JsVariable}()!.format({MomentDateDtoFormat(prop.Type)});");
                }
                else if (prop.IsForeignKey)
                {
                    var navigationProp = prop.ReferenceNavigationProperty;

                    b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}();");
                    // If the Id isn't set, use the object and see if that is set. Allows a child to get an Id after the fact.
                    using (b.Block($"if (!dto.{prop.JsonName} && this.{navigationProp.JsVariable}())"))
                    {
                        b.Line($"dto.{prop.JsonName} = this.{navigationProp.JsVariable}()!.{navigationProp.Object.PrimaryKey.JsVariable}();");
                    }
                }
                else if (prop.Type.IsCollection)
                {
                    if (prop.PureType.IsPOCO)
                    {
                        b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}()?.map(x => x.saveToDto());");
                    }
                    else
                    {
                        b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}();");
                    }
                }
                else if (prop.Object != null)
                {
                    b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}()?.saveToDto();");
                }
                else
                {
                    b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}();");
                }
            }
            b.Line();
            b.Line($"return dto;");
        }
    }




}

using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Knockout.Generators;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators
{
    public abstract class KnockoutVMGenerator : RazorTemplateGenerator<ClassViewModel>
    {
        public KnockoutVMGenerator(RazorTemplateServices razorServices) : base(razorServices) { }

        /// <summary>
        /// If true, code emitted for methods that load their parents' type should
        /// load the method's parent with the results of the method when called.
        /// </summary>
        protected bool MethodsLoadParent { get; set; }

        /// <summary>
        /// If true, calls to this.parent.load() will pass null as the first arg and a callback as the second.
        /// </summary>
        protected bool ParentLoadHasIdParameter { get; set; }

        public string ClientMethodDeclaration(MethodViewModel method, string parentClassName, int indentLevel = 2)
        {
            var b = new TypeScriptCodeBuilder(initialLevel: indentLevel, indentSize: 4);
            var model = this.Model;
            var isService = method.Parent.IsService;
            var returnIsListResult = method.ReturnsListResult;

            string reloadArg = !isService ? ", reload" : "";
            string reloadParam = !isService ? ", reload: boolean = true" : "";
            string callbackAndReloadParam = $"callback?: (result: {method.ResultType.TsType}) => void{reloadParam}";

            // Default instance of the method class.
            b.Line();
            b.Line("/**");
            b.Indented($"Methods and properties for invoking server method {method.Name}.");
            if (method.Comment.Length > 0) b.Indented(method.Comment);
            b.Line($"*/");
            var methodObjectName = method.JsVariable; // $"${method.JsVariable}"
            b.Line($"public readonly {method.JsVariable} = new {parentClassName}.{method.Name}(this);");

            // Not wrapping this in a using since it is used by nearly this entire method. Will manually dispose.
            string methodClass = returnIsListResult
                ? "ClientListMethod"
                : "ClientMethod";

            var classBlock = b.Block(
                $"public static {method.Name} = class {method.Name} extends Coalesce.{methodClass}<{parentClassName}, {method.ResultType.TsType}>", ';');

            b.Line($"public readonly name = '{method.Name}';");
            b.Line($"public readonly verb = '{method.ApiActionHttpMethodName}';");

           if (method.ResultType.IsCollection || method.ResultType.IsArray)
            {
                b.Line($"public result: {method.ResultType.TsKnockoutType()} = {method.ResultType.ObservableConstructorCall()};");
            }

            // Standard invoke method - all CS method parameters as TS method parameters.
            b.Line();
            b.Line($"/** Calls server method ({method.Name}) with the given arguments */");

            string parameters = "";
            parameters = string.Join(", ", method.ClientParameters.Select(f => f.Type.TsDeclarationPlain(f.Name) + " | null"));
            if (!string.IsNullOrWhiteSpace(parameters)) parameters = parameters + ", ";
            parameters = parameters + callbackAndReloadParam;

            using (b.Block($"public invoke = ({parameters}): JQueryPromise<any> =>", ';'))
            {
                b.Line($"return this.invokeWithData({method.JsPostObject}, callback{reloadArg});");
            }


            // Args class, default instance, invokeWithArgs method.
            if (method.ClientParameters.Any())
            {
                b.Line();

                b.Line($"/** Object that can be easily bound to fields to allow data entry for the method's parameters */");
                b.Line($"public args = new {method.Name}.Args(); ");

                using (b.Block("public static Args = class Args", ';'))
                    foreach (var arg in method.ClientParameters)
                    {
                        b.Line($@"public {arg.JsVariable}: {arg.Type.TsKnockoutType(true)} = {arg.Type.ObservableConstructorCall()};");
                    }

                b.Line();
                b.Line($"/** Calls server method ({method.Name}) with an instance of {method.Name}.Args, or the value of this.args if not specified. */");
                // We can't explicitly declare the type of the args parameter here - TypeScript doesn't allow it.
                // Thankfully, we can implicitly type using the default.
                using (b.Block($"public invokeWithArgs = (args = this.args, {callbackAndReloadParam}): JQueryPromise<any> =>"))
                {
                    b.Line($"return this.invoke({method.JsArguments("args", true)}{reloadArg});");
                }

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
                    b.Line($"return this.invoke({method.JsArguments("", true)}{reloadArg});");
                }
            }

            // Method response handler - highly dependent on what the response type actually is.
            b.Line();
            using (b.Block($"protected loadResponse = (data: Coalesce.{(returnIsListResult ? "List" : "Item")}Result, {callbackAndReloadParam}) =>", ';'))
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
                    
                     b.Line($"Coalesce.KnockoutUtilities.RebuildArray(this.result, {incomingMainData}, {keyNameArg}, ViewModels.{method.ResultType.PureType.ClassViewModel.Name}, this, true);");
                }
                else if (method.ResultType.HasClassViewModel)
                {
                    // Single view model return type.

                    b.Line("if (!this.result()) {");
                    b.Indented($"this.result(new ViewModels.{method.ResultType.PureType.ClassViewModel.Name}({incomingMainData}));");
                    b.Line("} else {");
                    b.Indented($"this.result().loadFromDto(data);");
                    b.Line("}");
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
                else if (method.ResultType.EqualsType(method.Parent.Type) && MethodsLoadParent)
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
                    b.Indented($"this.parent.load({(ParentLoadHasIdParameter ? "null, " : "")}typeof(callback) == 'function' ? () => callback(result) : undefined);");
                    b.Line("} else if (typeof(callback) == 'function') {");
                    // If we're not reloading, just call the callback now.
                    b.Indented("callback(this.result());");
                    b.Line("}");
                }
            }

            // End of the method class declaration.
            classBlock.Dispose();

            // Backwards compatibility for the old method call members (this will have a name conflict with the method object)
            // Keeping this in the code so it will still exist in the code history somewhere, but can probably be removed.
            // We collectively decided that we would introduce this significant breaking change in 2.0.0 and not maintain backwards-compat.

            //sb.Line($"/** Call server method ({method.Name}) */");
            //sb.Line($"public get {method.JsVariable}() {{ return this.{methodObjectName}.invoke; }}");
            //sb.Line($"/** Result of server method ({method.Name}) strongly typed in a observable. */");
            //sb.Line($"public get {method.JsVariableResult}() {{ return this.{methodObjectName}.result; }}");
            //sb.Line($"/** Raw result object of server method ({method.Name}) simply wrapped in an observable. */");
            //sb.Line($"public get {method.JsVariableResultRaw}() {{ return this.{methodObjectName}.rawResult; }}");
            //sb.Line($"/** True while the server method ({method.Name}) is being called */");
            //sb.Line($"public get {method.JsVariableIsLoading}() {{ return this.{methodObjectName}.isLoading; }}");
            //sb.Line($"/** Error message for server method ({method.Name}) if it fails. */");
            //sb.Line($"public get {method.JsVariableMessage}() {{ return this.{methodObjectName}.message; }}");
            //sb.Line($"/** True if the server method ({method.Name}) was successful. */");
            //sb.Line($"public get {method.JsVariableWasSuccessful}() {{ return this.{methodObjectName}.wasSuccessful; }}");
            //if (method.ClientParameters.Any())
            //{
            //    sb.Line($"/** Variable for method arguments to allow for easy binding. */");
            //    sb.Line($"public get {method.JsVariableWithArgs}() {{ return this.{methodObjectName}.invokeWithArgs; }}");
            //    sb.Line($"public get {method.JsVariableArgs}() {{ return this.{methodObjectName}.args }}");
            //    sb.Line($"public set {method.JsVariableArgs}(value) {{ this.{methodObjectName}.args = value; }}");
            //}

            return b.ToString();
        }
        public string SaveToDto(int indentLevel = 2)
        {
            var b = new TypeScriptCodeBuilder(indentLevel, indentSize: 4);
            b.Line($"/** Saves this object into a data transfer object to send to the server. */");
            using (b.Block($"public saveToDto = (): any =>"))
            {
                b.Line("var dto: any = {};");
                if (Model.PrimaryKey != null)
                {
                    b.Line($"dto.{Model.PrimaryKey.JsonName} = this.{Model.PrimaryKey.JsVariable}();");
                }
                b.Line($"");
                foreach (PropertyViewModel prop in Model.ClientProperties.Where(f => f.IsClientWritable && !f.IsPOCO))
                {
                    if (prop.Type.IsDate)
                    {
                        b.Line($"if (!this.{prop.JsVariable}()) dto.{prop.JsonName} = null;");
                        b.Line($"else dto.{prop.JsonName} = this.{prop.JsVariable}()!.format('YYYY-MM-DDTHH:mm:ss{(prop.Type.IsDateTimeOffset ? "ZZ" : "")}');");
                    }
                    else if (prop.IsForeignKey)
                    {
                        b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}();");
                        if (prop.IdPropertyObjectProperty != null && !prop.IsPrimaryKey)
                        {
                            // If the Id isn't set, use the object and see if that is set. Allows a child to get an Id after the fact.
                            using (b.Block($"if (!dto.{prop.JsonName} && this.{prop.IdPropertyObjectProperty.JsVariable}())"))
                            {
                                b.Line($"dto.{prop.JsonName} = this.{prop.IdPropertyObjectProperty.JsVariable}()!.{prop.IdPropertyObjectProperty.Object.PrimaryKey.JsVariable}();");
                            }
                        }
                    }
                    else if (!prop.Type.IsCollection)
                    {
                        b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}();");
                    }
                }
                b.Line();
                b.Line($"return dto;");
            }
            return b.ToString();
        }
    }




}

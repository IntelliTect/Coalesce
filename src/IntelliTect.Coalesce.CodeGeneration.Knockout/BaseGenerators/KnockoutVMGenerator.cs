using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Knockout.Generators;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
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
            var sb = new CodeBuilder(initialLevel: indentLevel);
            var model = this.Model;
            
            // Not wrapping this in a using since it is used by nearly this entire method. Will manually dispose.
            var classBlock = sb.TSBlock(
                $"public static {method.Name} = class {method.Name} extends Coalesce.ClientMethod<{parentClassName}, {method.ReturnType.TsType}>", true);

            sb.Line($"public readonly name = '{method.Name}';");
            sb.Line($"public readonly verbme = '{method.ApiAame}';");

            // Standard invoke method - all CS method parameters as TS method parameters.
            sb.Line();
            sb.Line($"/** Calls server method ({method.Name}) with the given arguments */");
            using (sb.TSBlock($"public invoke = ({method.TsParameters}): JQueryPromise<any> =>", true))
            {
                sb.Line($"return this.invokeWithData({method.JsPostObject}, callback, reload);");
            }


            // Args class, default instance, invokeWithArgs method.
            if (method.ClientParameters.Any())
            {
                sb.Line();

                using (sb.TSBlock("public static Args = class Args", true))
                foreach (var arg in method.ClientParameters)
                {
                    sb.Line($@"public {arg.CsArgumentName}: {arg.Type.TsKnockoutType()} = {arg.Type.ObservableConstructorCall()};");
                }

                sb.Line();
                sb.Line($"/** Calls server method ({method.Name}) with an instance of {method.Name}.Args, or the value of this.args if not specified. */");
                // We can't explicitly declare the type of the args parameter here - TypeScript doesn't allow it.
                // Thankfully, we can implicitly type using the default.
                using (sb.TSBlock($"public invokeWithArgs = (args = this.args, callback?: (result: {method.ReturnType.TsType}) => void, reload: boolean = true) =>"))
                {
                    sb.Line($"return this.invoke({method.JsArguments("args", true)}, reload);");
                }
                sb.Line();

                sb.Line($"/** Object that can be easily bound to fields to allow data entry for the method's parameters */");
                sb.Line($"public args = new {method.Name}.Args(); ");
            }

            // Method response handler - highly dependent on what the response type actually is.
            sb.Line();
            using (sb.TSBlock($"protected loadResponse = (data: any, callback?: (result: {method.ReturnType.TsType}) => void, reload?: boolean) =>", true))
            {
                if (method.ReturnType.IsCollection && method.ReturnType.PureType.HasClassViewModel) {
                    // Collection of objects that have TypeScript ViewModels. This could be an entity or an external type.

                    // If the models have a key, rebuild our collection using that key so that we can reuse objects when the data matches.
                    var keyNameArg = method.ReturnType.PureType.ClassViewModel.PrimaryKey != null
                        ? $"'{method.ReturnType.PureType.ClassViewModel.PrimaryKey.JsVariable}'"
                        : "null";

                    sb.Line($"Coalesce.KnockoutUtilities.RebuildArray(this.result, data, {keyNameArg}, ViewModels.{method.ReturnType.PureType.ClassViewModel.Name}, this, true);");
                }
                else if (method.ReturnType.HasClassViewModel)
                {
                    // Single view model return type.

                    sb.Line("if (!this.result()) {");
                    sb.Indented($"this.result(new ViewModels.{method.ReturnType.PureType.ClassViewModel.Name}(data));");
                    sb.Line("} else {");
                    sb.Indented($"this.result().loadFromDto(data);");
                    sb.Line("}");
                }
                else {
                    // Uninteresting return type. Either void, a primitive, or a collection of primitives.
                    // In any case, regardless of the type of the 'result' observable, this is how we set the results.
                    sb.Line("this.result(data);");
                }

                if (method.ReturnType.EqualsType(method.Parent.Type) && !MethodsLoadParent)
                {
                    // The return type is the type of the method's parent. Load the parent with the results of the method.
                    // Parameter 'reload' has no meaning here, since we're reloading the object with the result of the method.
                    sb.Line($"this.parent.loadFromDto(data, true)");
                    using (sb.TSBlock("if (typeof(callback) == 'function')"))
                    {
                        sb.Line($"callback(this.result());");
                    }
                }
                else
                {
                    // We're not loading the results into the method's parent. This is by far the more common case.
                    sb.Line("if (typeof(callback) != 'function') return;");

                    sb.Line("if (reload) {");
                    // If reload is true, invoke the load function on the method's parent to reload its latest state from the server.
                    // Only after that's done do we call the method-completion callback.
                    // Store the result locally in case it somehow gets changed by us reloading the parent.
                    sb.Indented("var result = this.result();");
                    sb.Indented($"this.parent.load({(ParentLoadHasIdParameter ? "null, " : "")}() => callback(result));");
                    sb.Line("} else {");
                    // If we're not reloading, just call the callback now.
                    sb.Indented("callback(this.result());");
                    sb.Line("}");
                }
            }
            
            sb.Line();
            sb.Line("/** Invokes the method after displaying a browser-native prompt for each argument. */");
            using (sb.TSBlock($"public invokeWithPrompts = (callback: (result: {method.ReturnType.TsType}) => void = null, reload: boolean = true): JQueryPromise<any> =>", true)){
                sb.Line($"var $promptVal: string = null;");
                foreach (var param in method.ClientParameters.Where(f => f.ConvertsFromJsString))
                {
                    sb.Line($"$promptVal = {$"prompt('{param.Name.ToProperCase()}')"};");
                    // AES 1/23/2018 - why do we do this? what about optional params where no value is desired?
                    sb.Line($"if ($promptVal === null) return;");
                    sb.Line($"var {param.Name}: {param.Type.TsType} = {param.Type.TsConvertFromString("$promptVal")};");
                }

                // For all parameters that can't convert from a string (is this even possible with what we support for method parameters now?),
                // pass null as the value. I guess we just let happen what's going to happen? Again, not sure when this case would ever be hit.
                foreach (var param in method.ClientParameters.Where(f => !f.ConvertsFromJsString))
                {
                    sb.Line($"var {param.Name}: {param.Type.TsType} = null;");
                }
                sb.Line($"return this.invoke({method.JsArguments("", true)}, reload);");
            }

            // End of the method class declaration.
            classBlock.Dispose();

            // Default instance of the method class.
            sb.Line();
            sb.Line("/**");
            sb.Indented($"Methods and properties for invoking server method {method.Name}.");
            if (method.Comment.Length > 0) sb.Indented(method.Comment);
            sb.Line($"*/");
            var methodObjectName = method.JsVariable; // $"${method.JsVariable}"
            sb.Line($"public readonly {method.JsVariable} = new {parentClassName}.{method.Name}(this);");
            sb.Line($"");
            
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

            return sb.ToString();
        }
    }
}

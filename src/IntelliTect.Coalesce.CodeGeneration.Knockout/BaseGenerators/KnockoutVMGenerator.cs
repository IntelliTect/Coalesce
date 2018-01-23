using IntelliTect.Coalesce.CodeGeneration.Generation;
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

        public string ClientMethodDeclaration(MethodViewModel method, int indentLevel = 2)
        {
            var sb = new IndentingStringBuilder(initialLevel: indentLevel);
            var model = this.Model;




            sb.Line($"public static @(method.Name.ToPascalCase()) = class @(method.Name.ToPascalCase()) extends Coalesce.ClientMethod<{model.ViewModelGeneratedClassName}, {method.ReturnType.TsType}> {{");
            sb.Line($"    public readonly name = '{method.Name.ToPascalCase()}';");
            sb.Line($"    ");
            sb.Line($"    /** Calls server method ({method.Name}) with the given arguments */");
            sb.Line($"    public invoke = ({method.TsParameters}): JQueryPromise<any> => {{");
            sb.Line($"        return this.invokeWithData({method.JsPostObject}, callback, reload);");
            sb.Line("    };");
            if (method.ClientParameters.Any())
            {
                sb.Line("");
                sb.Line($@"    public static Args = class Args {{");
                foreach (var arg in method.ClientParameters){
                    sb.Line($@"        public {arg.CsArgumentName}: {arg.Type.TsKnockoutType()} = {arg.Type.ObservableConstructorCall()};");
                }
                sb.Line($@"    }}");
                sb.Line($@"    /** Calls server method ({method.Name}) with an instance of {method.Name.ToPascalCase()}.Args, or the value of this.args if not specified. */");
                sb.Line($@"    // We can't explicitly declare the type of the args parameter here - TypeScript doesn't allow it. Thankfully, we can implicitly type using the default.");
                sb.Line($@"    public invokeWithArgs = (args = this.args, callback?: (result: {method.ReturnType.TsType}) => void, reload: boolean = true) => {{");
                sb.Line($@"        return this.invoke({method.JsArguments("args", true)}, reload);");
                sb.Line($@"    }}");
                sb.Line($@"    /** Object that can be easily bound to fields to allow data entry for the method */");
                sb.Line($@"    public args = new @(method.Name.ToPascalCase()).Args(); ");
                }
            sb.Line($@"    ");
            sb.Line($@"    protected loadResponse = (data: any, callback?: (result: {method.ReturnType.TsType}) => void, reload?: boolean) => {{");
            if (method.ReturnType.IsCollection && method.ReturnType.PureType.HasClassViewModel){
                var keyNameArg = method.ReturnType.PureType.ClassViewModel.PrimaryKey != null
                    ? $"'{method.ReturnType.PureType.ClassViewModel.PrimaryKey.JsVariable}'"
                    : "null";
                sb.Line($@"        Coalesce.KnockoutUtilities.RebuildArray(this.result, data, {keyNameArg}, ViewModels.{method.ReturnType.PureType.ClassViewModel.Name}, this, true);");
            }
            else if (method.ReturnType.HasClassViewModel)
            {
                sb.Line(@"        if (!this.result()){");
                sb.Line($@"            this.result(new {method.ReturnType.ClassViewModel.ViewModelClassName}(data));");
                sb.Line(@"        } else {");
                sb.Line($@"            this.result().loadFromDto(data);");
                sb.Line("        }");
            }
            else {
                sb.Line($@"        this.result(data);");
            }

            if (method.ReturnType.EqualsType(method.Parent.Type))
            {
                sb.Line($@"        // The return type is the type of the object, load it.");
                sb.Line($@"        this.parent.loadFromDto(data, true)");
                sb.Line(@"        if (typeof(callback) == 'function') {");
                sb.Line($@"            var result = this.result();");
                sb.Line($@"            callback(result);");
                sb.Line("        }");
            }
            else
            {
                sb.Line("        if (typeof(callback) != 'function') return;");
                sb.Line("        var result = this.result();");
                sb.Line("        if (reload) {");
                sb.Line("          this.parent.load(null, () => callback(result));");
                sb.Line("        } else {");
                sb.Line("          callback(result);");
                sb.Line("        }");
            }
            sb.Line("    };");
            sb.Line("};");
            sb.Line("");



            sb.Line("/**");
            sb.Line($"    Methods and properties for invoking server method {method.Name}.");
            if (method.Comment.Length > 0)
            {
            sb.Line(method.Comment);
            }
            sb.Line($"*/");

            sb.Line($"public readonly ${method.JsVariable} = new {model.ViewModelClassName}.{method.Name.ToPascalCase()}(this);");
            sb.Line($"");
            sb.Line($"/** Call server method ({method.Name}) */");
            sb.Line($"public get {method.JsVariable}() {{ return this.${method.JsVariable}.invoke; }}");
            sb.Line($"/** Result of server method ({method.Name}) strongly typed in a observable. */");
            sb.Line($"public get {method.JsVariableResult}() {{ return this.${method.JsVariable}.result; }}");
            sb.Line($"/** Raw result object of server method ({method.Name}) simply wrapped in an observable. */");
            sb.Line($"public get {method.JsVariableResultRaw}() {{ return this.${method.JsVariable}.rawResult; }}");
            sb.Line($"/** True while the server method ({method.Name}) is being called */");
            sb.Line($"public get {method.JsVariableIsLoading}() {{ return this.${method.JsVariable}.isLoading; }}");
            sb.Line($"/** Error message for server method ({method.Name}) if it fails. */");
            sb.Line($"public get {method.JsVariableMessage}() {{ return this.${method.JsVariable}.message; }}");
            sb.Line($"/** True if the server method ({method.Name}) was successful. */");
            sb.Line($"public get {method.JsVariableWasSuccessful}() {{ return this.${method.JsVariable}.wasSuccessful; }}");

            if (method.ClientParameters.Any())
            {
                sb.Line($":/** Variable for method arguments to allow for easy binding. */");
                sb.Line($":public get {method.JsVariableWithArgs}() {{ return this.${method.JsVariable}.invokeWithArgs; }}");
                sb.Line($":public get {method.JsVariableArgs}() {{ return this.${method.JsVariable}.args }}");
                sb.Line($":public set {method.JsVariableArgs}(value) {{ this.${method.JsVariable}.args = value; }}");
            }


            sb.Line($"public {method.JsVariableUi} = (callback: () => void = null, reload: boolean = true): JQueryPromise<any> => {{");
            using (sb.Block()){
                sb.Line($"    var $promptVal: string = null;");
                foreach (var param in method.ClientParameters.Where(f => f.ConvertsFromJsString))
                {
                    sb.Line($"    $promptVal = {$"prompt('{param.Name.ToProperCase()}')"};");
                    sb.Line($"    if ($promptVal === null) return;");
                    sb.Line($"    var {param.Name}: {param.Type.TsType} = {param.Type.TsConvertFromString("$promptVal")};");
                    sb.Line($"");
                }
                foreach (var param in method.ClientParameters.Where(f => !f.ConvertsFromJsString))
                {
                    sb.Line($"    var {param.Name}: {param.Type.TsType} = null;");
                }
                sb.Line($"    return this.{method.JsVariable}({method.JsArguments("", true)}, reload);");
            }
            sb.Line("}");

            return sb.ToString();
        }
    }
}

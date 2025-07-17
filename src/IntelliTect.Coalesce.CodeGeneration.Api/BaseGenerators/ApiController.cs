using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators
{
    public abstract class ApiController : ApiService
    {
        public ApiController(GeneratorServices services) : base(services)
        {
        }

        /// <summary>
        /// Writes the common set of namespaces used by API controllers.
        /// </summary>
        protected void WriteNamespaces(CSharpCodeBuilder b)
        {
            var namespaceName = GetAreaNamespace();

            var namespaces = new[]
            {
                "IntelliTect.Coalesce",
                "IntelliTect.Coalesce.Api",
                "IntelliTect.Coalesce.Api.Controllers",
                "IntelliTect.Coalesce.Api.DataSources",
                "IntelliTect.Coalesce.Api.Behaviors",
                "IntelliTect.Coalesce.Mapping",
                "IntelliTect.Coalesce.Mapping.IncludeTrees",
                "IntelliTect.Coalesce.Models",
                "IntelliTect.Coalesce.TypeDefinition",
                "Microsoft.AspNetCore.Authorization",
                "Microsoft.AspNetCore.Mvc",
                "Microsoft.AspNetCore.Http",
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.ComponentModel.DataAnnotations",
                "System.Net",
                "System.Threading.Tasks",

                // This is the output namespace for the generated DTOs
                $"{namespaceName}.Models"
            };

            b.Line();
            foreach (var ns in namespaces.Where(n => !string.IsNullOrEmpty(n)).OrderBy(n => n))
            {
                b.Line($"using {ns};");
            }
        }

        /// <summary>
        /// Writes the route attribute for an API controller.
        /// If the API controller has routing disabled, nothing will be written.
        /// </summary>
        protected void WriteControllerRouteAttribute(CSharpCodeBuilder b)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (!Model.ApiRouted) return;
#pragma warning restore CS0618 // Type or member is obsolete

            if (!string.IsNullOrWhiteSpace(AreaName))
            {
                b.Line($"[Route(\"{AreaName}/api/{Model.ApiRouteControllerPart}\")]");
            }
            else
            {
                b.Line($"[Route(\"api/{Model.ApiRouteControllerPart}\")]");
            }
        }

        private static void WriteControllerActionPreamble(CSharpCodeBuilder b, MethodViewModel method)
        {
            var methodAnnotationName = $"Http{method.ApiActionHttpMethod}";

            b.DocComment($"Method: {method.Name}");
            b.Line($"[{methodAnnotationName}(\"{method.NameWithoutAsync}\")]");
            if (method.Name != method.NameWithoutAsync)
            {
                // Add a route attribute that includes "Async" if it exists in the method name
                // for backwards compatibility (3.0 breaking change).
                b.Line($"[{methodAnnotationName}(\"{method.Name}\")]");
            }
            b.Line(method.SecurityInfo.Execute.MvcAnnotation());
        }

        private static void WriteControllerActionJsonPreamble(CSharpCodeBuilder b, MethodViewModel method)
        {
            if (!method.ApiParameters.Any()) return;

            b.Line();
            using (b.Block($"public class {method.ParameterClassName}"))
            {
                foreach (var param in method.ApiParameters.OrderBy(p => p.HasDefaultValue))
                {
                    string typeName;
                    if (param.PureType.IsFile)
                    {
                        typeName = "IntelliTect.Coalesce.Models.FileParameter";
                        if (param.Type.IsCollection) typeName = $"ICollection<{typeName}>";
                    }
                    else
                    {
                        typeName = param.Type.NullableTypeForDto(isInput: true, dtoNamespace: null, dontEmitNullable: true);
                    }

                    b.Append("public ");
                    b.Append(typeName);
                    b.Append(" ");
                    b.Append(param.PascalCaseName);
                    b.Append(" { get; set; }");
                    if (param.HasDefaultValue)
                    {
                        b.Append(" = ");
                        b.Append(param.CsDefaultValue);
                        b.Append(";");
                    }
                    b.Line();
                }
            }

            WriteControllerActionPreamble(b, method);
        }

        private void WriteControllerActionReturnSignature(CSharpCodeBuilder b, MethodViewModel method)
        {
            var returnType = method.ApiActionReturnTypeDeclaration;
            if (method.ResultType.IsFile)
            {
                // Wrap the signature in ActionResult<> since file-returning methods
                // have mixed return types due to use of the File() method on controllers.
                // This is not baked into ApiActionReturnTypeDeclaration because it ONLY affects
                // the signature, but not the places within the method that we instantiate the return type
                // due to the implcit conversions from T to ActionResult<T>.
                returnType = $"ActionResult<{returnType}>";
            }
            if (method.IsAwaitable || method.IsModelInstanceMethod || method.Parameters.Any(p => p.Type.IsByteArray))
            {
                returnType = $"async Task<{returnType}>";
            }

#pragma warning disable CS0618 // Type or member is obsolete
            b.Append(Model.ApiActionAccessModifier);
#pragma warning restore CS0618 // Type or member is obsolete
            b.Append(" virtual ");
            b.Append(returnType);
            b.Append(" ");
            b.Append(method.NameWithoutAsync);
        }

        protected IDisposable WriteControllerActionSignature(CSharpCodeBuilder b, MethodViewModel method)
        {
            WriteControllerActionPreamble(b, method);

            if (method.HasHttpRequestBody)
            {
                b.Line("""[Consumes("application/x-www-form-urlencoded", "multipart/form-data")]""");
            }

            WriteControllerActionReturnSignature(b, method);

            b.Line("(");
            using var indent = b.Indented();

            if (method.IsModelInstanceMethod)
            {
                b.Line("[FromServices] IDataSourceFactory dataSourceFactory,");
            }

            foreach (var param in method.Parameters
                .Where(f => f.IsDI && !f.IsNonArgumentDI)
                .Concat(method.ApiParameters)
                .OrderBy(p => p.HasDefaultValue))
            {
                string typeName;
                if (param.PureType.IsFile)
                {
                    typeName = param.Type.IsCollection
                        ? new ReflectionTypeViewModel(typeof(ICollection<Microsoft.AspNetCore.Http.IFormFile>)).FullyQualifiedName
                        : new ReflectionTypeViewModel(typeof(Microsoft.AspNetCore.Http.IFormFile)).FullyQualifiedName;
                }
                else if (param.IsDI)
                {
                    typeName = param.Type.FullyQualifiedName;
                }
                else
                {
                    typeName = param.Type.NullableTypeForDto(isInput: true, dtoNamespace: null, dontEmitNullable: true);
                }

                if (param.ShouldInjectFromServices)
                {
                    b.Append("[FromServices] ");
                }
                else if (param.IsDI)
                {
                    // Do nothing. This is a non-service injection (i.e. ClaimsPrincipal or CancellationToken)
                }
                else if (param.PureType.IsFile)
                {
                    // File parameters must not be annotated with FromForm, as this will break their model binding.
                    // They have their own special implicit model binder.
                }
                else if (method.HasHttpRequestBody)
                {
                    // We must add [FromForm], because without it, AspNetCore's ApiExplorer, and subsequently Swashbuckle,
                    // will assume these parameters to be from the querystring. This is well defined behavior in aspnetcore,
                    // but is not what we ever want (https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0#sources-1).

                    // Furthermore, we must specify a Name because without it, top level parameters are ambiguous with nested parameters.
                    // For example, for a custom method that takes a `class Case { string Name }` parameter and a `string name` parameter,
                    // if the Case is passed as null and a name is passed a value, both the `name` parameter and `Case.Name` will be set to `name`.
                    b.Append("[FromForm(Name = ").Append(param.JsVariable.QuotedStringLiteralForCSharp()).Append(")] ");
                }
                else
                {
                    b.Append("[FromQuery] ");
                }

                b.Append(typeName);
                b.Append(" ");
                b.Append(param.CsParameterName);
                if (param.HasDefaultValue)
                {
                    b.Append(" = ");
                    b.Append(param.CsDefaultValue);
                }
                b.Line(",");
            }

            b.TrimWhitespace().TrimEnd(",").Append(")");
            indent.Dispose();
            var blockRet = b.Block();

            WriteMethodViewModelVar(b, method);
            WriteFormDataParamsObject(b, method);

            return blockRet;
        }

        protected IDisposable WriteControllerActionJsonSignature(CSharpCodeBuilder b, MethodViewModel method)
        {
            WriteControllerActionJsonPreamble(b, method);

            if (method.ApiParameters.Any())
            {
                b.Line("[Consumes(\"application/json\")]");
            }

            WriteControllerActionReturnSignature(b, method);

            b.Line("(");
            using var indent = b.Indented();

            if (method.IsModelInstanceMethod)
            {
                b.Line("[FromServices] IDataSourceFactory dataSourceFactory,");
            }

            foreach (var param in method.Parameters
                .Where(f => f.IsDI && !f.IsNonArgumentDI))
            {
                string typeName = param.Type.FullyQualifiedName;

                if (param.ShouldInjectFromServices)
                {
                    b.Append("[FromServices] ");
                }

                b.Append(typeName);
                b.Append(" ");
                b.Append(param.CsParameterName);
                b.Line(",");
            }

            if (method.ApiParameters.Any())
            {
                b.Line($"[FromBody] {method.ParameterClassName} _params");
            }

            indent.Dispose();
            b.Line(")");

            var blockRet = b.Block();

            WriteMethodViewModelVar(b, method);

            return blockRet;
        }

        private void WriteFormDataParamsObject(CSharpCodeBuilder b, MethodViewModel method)
        {
            var clientParameters = method.ApiParameters.ToList();
            if (clientParameters.Count == 0) return;

            using (b.Block("var _params = new", ";"))
            {
                for (int i = 0; i < clientParameters.Count; i++)
                {
                    var param = clientParameters[i];
                    if (i != 0) b.Line(", ");
                    b.Append(param.PascalCaseName);
                    b.Append(" = ");
                    b.Append(GetMethodParameterFormDataMappingExpression(method, clientParameters[i]));
                }
            }
            b.Line();
        }


        /// <summary>
        /// For a method invocation controller action, builds an expression for the given parameter
        /// that maps the controller input to the argument of method being called.
        /// Does not perform DTO mapping, as this method produces the target of validation.
        /// </summary>
        protected string GetMethodParameterFormDataMappingExpression(MethodViewModel method, ParameterViewModel param)
        {
            var ret = param.CsParameterName;

            if (param.PureType.IsFile)
            {
                ret = $"{ret} == null ? null : " + (param.Type.IsCollection
                    ? $"{ret}.Select(f => {FileCtor("f")})"
                    : $"{FileCtor(ret)} ");

                static string FileCtor(string x) =>
                    $"new IntelliTect.Coalesce.Models.File {{ Name = {x}.FileName, ContentType = {x}.ContentType, Length = {x}.Length, Content = {x}.OpenReadStream() }}";
            }

            if (param.Type.IsByteArray && method.HasHttpRequestBody)
            {
                // For binary data posted as a JS Blob, it comes across as a file.
                // Such files must be manually pulled out of the form data
                // because the model binder by default will only bind byte[]
                // as base64 data. However, we want to support receiving either
                // raw binary OR base64 without knowing ahead of time what we'll get.
                ret += $" ?? await ((await Request.ReadFormAsync()).Files[{param.JsVariable.QuotedStringLiteralForCSharp()}]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<{param.Type.FullyQualifiedName}>(null))";
            }

            if (param.Type.IsCollection)
            {
                ret += ".ToList()";
            }

            if (param.PureType.HasClassViewModel)
            {
                // Object parameters still get instantiated by the aspnetcore model binder,
                // even if they were passed as null from the frontend.
                // We therefore need to do some work to undo that behavior so that what the
                // method is called with from the client typescript is what ends up in C#.
                // https://github.com/IntelliTect/Coalesce/issues/456

                ret = $"!Request.Form.HasAnyValue({param.JsVariable.QuotedStringLiteralForCSharp()}) ? null : {ret}";
            }

            return ret;
        }
    }
}

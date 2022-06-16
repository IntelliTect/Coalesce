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
    public abstract class ApiController : StringBuilderCSharpGenerator<ClassViewModel>
    {
        public ApiController(GeneratorServices services) : base(services)
        {
        }

        /// <summary>
        /// Writes the common set of namespaces used by API controllers.
        /// </summary>
        protected string WriteNamespaces(CSharpCodeBuilder b)
        {
            string namespaceName = Namespace;
            if (!string.IsNullOrWhiteSpace(AreaName))
            {
                namespaceName += "." + AreaName;
            }
            var namespaces = new[]
            {
                "IntelliTect.Coalesce",
                "IntelliTect.Coalesce.Api",
                "IntelliTect.Coalesce.Api.Controllers",
                "IntelliTect.Coalesce.Api.DataSources",
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

            return namespaceName;
        }

        /// <summary>
        /// Writes the route attribute for an API controller.
        /// If the API controller has routing disabled, nothing will be written.
        /// </summary>
        protected void WriteControllerRouteAttribute(CSharpCodeBuilder b)
        {
            if (Model.ApiRouted)
            {
                if (!string.IsNullOrWhiteSpace(AreaName))
                {
                    b.Line($"[Route(\"{AreaName}/api/{Model.ApiRouteControllerPart}\")]");
                }
                else
                {
                    b.Line($"[Route(\"api/{Model.ApiRouteControllerPart}\")]");
                }
            }
        }

        protected static void WriteControllerActionPreamble(CSharpCodeBuilder b, MethodViewModel method)
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


        protected IDisposable WriteControllerActionSignature(CSharpCodeBuilder b, MethodViewModel method)
        {
            var parameters = method.Parameters.Where(f => !f.IsNonArgumentDI).ToArray();
            var actionParameters = new List<string>();

            if (method.IsModelInstanceMethod)
            {
                actionParameters.Add("[FromServices] IDataSourceFactory dataSourceFactory");
            }

            actionParameters.AddRange(method.Parameters
                .Where(f => f.IsDI && !f.IsNonArgumentDI)
                .Concat(method.ApiParameters)
                .OrderBy(p => p.HasDefaultValue)
                .Select(param =>
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
                    typeName = param.Type.DtoFullyQualifiedName;
                }

                return $"{(param.ShouldInjectFromServices ? "[FromServices] " : "")}{typeName} {param.CsParameterName}{(param.HasDefaultValue ? " = " + param.CsDefaultValue : "")}";
            }));

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

            return b.Block($"{Model.ApiActionAccessModifier} virtual {returnType} {method.NameWithoutAsync} ({string.Join(", ", actionParameters)})");
        }

        public const string MethodResultVar = "_methodResult";
        public const string MappingContextVar = "_mappingContext";

        /// <summary>
        /// For a method invocation controller action, writes the actual invocation of the method.
        /// </summary>
        /// <param name="b">The CodeBuilder to write to</param>
        /// <param name="method">The method to be invoked</param>
        /// <param name="owningMember">
        /// The member on which to invoke the method. 
        /// This could be an variable holding an instance of a type, or a class reference if the method is static.
        /// </param>
        public void WriteMethodInvocation(CSharpCodeBuilder b, MethodViewModel method, string owningMember)
        {
            if (method.ResultType.PureType.ClassViewModel?.IsDto == false)
            {
                b.Line("IncludeTree includeTree = null;");
            }

            if (method.Parameters.Any(p => !p.IsDI && p.PureType.HasClassViewModel)
                || method.ResultType.PureType.HasClassViewModel
            )
            {
                b.Line($"var {MappingContextVar} = new {nameof(MappingContext)}(User);");
            }

            // Don't try to store the result in the variable if the method returns void.
            if (!method.TaskUnwrappedReturnType.IsVoid)
            {
                b.Append($"var {MethodResultVar} = ");
            }

            var awaitSymbol = method.IsAwaitable ? "await " : "";
            b.Append($"{awaitSymbol}{owningMember}.{method.Name}(");

            var parameters = method.Parameters.ToList();
            using (b.Indented())
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (i != 0) b.Append(", ");
                    //b.Line(); // Uncomment for parameter-per-line
                    b.Append(GetMethodInvocationParameterExpression(parameters[i]));
                }
            }
            //if (parameters.Any()) b.Line(); // Uncomment for parameter-per-line
            b.Line(");");
        }


        /// <summary>
        /// For a method invocation controller action, builds an expression for the given parameter
        /// that maps the controller input to the argument of method being called.
        /// </summary>
        public string GetMethodInvocationParameterExpression(ParameterViewModel param)
        {
            if (param.IsNonArgumentDI)
            {
                // We expect these to either be present on the controller which we're generating for,
                // or in the contents of the generated action method.
                if (param.IsAutoInjectedContext) return "Db";
                else if (param.IsAUser) return "User";
                else if (param.IsAnIncludeTree) return "out includeTree";
                else throw new ArgumentException("Unknown type of NonArgumentDI");
            }

            if (param.IsDI)
            { 
                return param.CsParameterName;
            }

            var ret = param.CsParameterName;

            if (param.PureType.IsFile)
            {
                ret = $"{ret} == null ? null : " + (param.Type.IsCollection
                    ? $"{ret}.Select(f => ({param.PureType.FullyQualifiedName}){FileCtor("f")})"
                    : $"{FileCtor(ret)} ");

                static string FileCtor(string x) =>
                    $"new IntelliTect.Coalesce.Models.File {{ Name = {x}.FileName, ContentType = {x}.ContentType, Length = {x}.Length, Content = {x}.OpenReadStream() }}";
            }

            if (param.Type.IsByteArray)
            {
                // For binary data posted as a JS Blob, it comes across as a file.
                // Such files must be manually pulled out of the form data
                // because the model binder by default will only bind byte[]
                // as base64 data. However, we want to support receiving either
                // raw binary OR base64 without knowing ahead of time what we'll get.
                ret += $" ?? await ((await Request.ReadFormAsync()).Files[nameof({param.CsParameterName})]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<{param.Type.FullyQualifiedName}>(null))";
            }

            if (param.Type.PureType.HasClassViewModel)
            {
                if (param.Type.IsCollection)
                {
                    ret = $"{param.CsParameterName}.Select(_m => _m.{nameof(Mapper.MapToModel)}(new {param.Type.PureType.FullyQualifiedName}(), _mappingContext))";
                }
                else
                {
                    ret = $"{param.CsParameterName}.{nameof(Mapper.MapToModel)}(new {param.Type.FullyQualifiedName}(), _mappingContext)";
                }
            }

            if (param.Type.IsCollection)
            {
                if (param.Type.IsArray)
                    ret += ".ToArray()";
                else
                    ret += ".ToList()";
            }

            return ret;
        }

        /// <summary>
        /// For a method invocation controller action, writes the block of code that 
        /// transforms the raw return result of the method into an object appropriate 
        /// for returning from the action and ultimately serialization to the client.
        /// </summary>
        /// <param name="b">The code builder to write to.</param>
        /// <param name="method">The method whose results need transforming.</param>
        public void WriteMethodResultProcessBlock(CSharpCodeBuilder b, MethodViewModel method)
        {
            var resultVar = MethodResultVar;
            var resultProp = "Object";
            var resultType = method.ResultType;

            // For methods that return ItemResult<> or ListResult<>, grab the core result data from the appropriate member.
            if (method.TaskUnwrappedReturnType.IsA(typeof(ItemResult<>)))
            {
                resultVar += ".Object";
            }
            else if (method.ReturnsListResult)
            {
                resultProp = "List";
                resultVar += ".List";
            }

            if (method.ResultType.IsFile)
            {
                using (b.Block($"if ({resultVar} != null)"))
                {
                    var fileNameVar = $"{resultVar}.{nameof(IFile.Name)}";
                    b.Line($"string _contentType = {resultVar}.{nameof(IFile.ContentType)};");

                    b.Line($"if (string.IsNullOrWhiteSpace(_contentType) && (");
                    b.Indented($"string.IsNullOrWhiteSpace({fileNameVar}) ||");
                    b.Indented($"!(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType({fileNameVar}, out _contentType))");
                    using (b.Block("))"))
                    {
                        b.Line($"_contentType = \"application/octet-stream\";");
                    }

                    var contentStreamVar = $"{resultVar}.{nameof(IFile.Content)}";
                    // Use range processing if the result stream isn't a MemoryStream.
                    // MemoryStreams are just going to mean we're dumping the whole byte array straight back to the client.
                    // Other streams might be more elegant, e.g. QueryableContentStream 
                    b.Line($"return File({contentStreamVar}, _contentType, {fileNameVar}, !({contentStreamVar} is System.IO.MemoryStream));");
                }

                if (!method.TaskUnwrappedReturnType.IsA<ApiResult>())
                {
                    using (b.Block($"else"))
                    {
                        // Method doesn't return an ItemResult, so has no way to explicitly signal failure
                        // aside from returning `null`. Return a 404, since otherwise we'd fall through below
                        // and serve a blank {wasSuccessful: true} response.
                        b.Line($"return NotFound();");
                    }
                }
            }

            if (method.TaskUnwrappedReturnType.IsA<ApiResult>())
            {
                // For any ApiResult return type, pass it to our ApiResult ctor to grab the WasSuccessful and Message props.
                // For list results, this also copies paging information.
                b.Line($"var _result = new {method.ApiActionReturnTypeDeclaration}({MethodResultVar});");
            }
            else
            {
                // Return type isn't an ApiResult - just create a 'blank' object.
                b.Line($"var _result = new {method.ApiActionReturnTypeDeclaration}();");
            }

            if (resultType.IsVoid)
            {
                // If the result type is void, there's no methodResult to stick on our return object.
                // We'll just return the plain 'result' object.
            }
            else if (resultType.IsA<ApiResult>())
            {
                // If the resultType (not the ReturnType) is an API result, this means that the method is either:
                // 1) Returning exactly ApiResult and not a derived type of it. This is OK (I guess...); in this case, we can safely ignore because there is no Object on it to process.
                // 2) Returning an ItemResult<T> or ListResult<T> where T : ApiResult. This is bonkers. Don't do this.
            }
            else if (resultType.IsCollection)
            {
                if (resultType.PureType.HasClassViewModel && !resultType.PureType.ClassViewModel.IsDto)
                {
                    // Return type is a collection of models that need to be mapped to a DTO.
                    // ToList the result (because it might be IQueryable - we need to execute the query before mapping)
                    b.Append($"_result.{resultProp} = {resultVar}?.ToList().Select(o => ");
                    b.Append($"Mapper.MapToDto<{resultType.PureType.ClassViewModel.FullyQualifiedName}, {resultType.PureType.ClassViewModel.DtoName}>");

                    // Only attempt to pull the include tree out of the result if the user actually typed their return type as an IQueryable.
                    var includeTreeForMapping = resultType.IsA<IQueryable>()
                        ? $"includeTree ?? ({resultVar} as IQueryable)?.GetIncludeTree()"
                        : "includeTree";

                    b.Append($"(o, {MappingContextVar}, {includeTreeForMapping}))");
                }
                else
                {
                    // Return type is a collection of primitives or IClassDtos.
                    b.Append($"_result.{resultProp} = {resultVar}?");
                }

                if (resultType.IsArray)
                    b.Line(".ToArray();");
                else
                    b.Line(".ToList();");
            }
            else
            {
                if (resultType.HasClassViewModel && !resultType.ClassViewModel.IsDto)
                {
                    // Return type is a model that needs to be mapped to a DTO.
                    b.Line($"_result.{resultProp} = Mapper.MapToDto<{resultType.ClassViewModel.FullyQualifiedName}, {resultType.ClassViewModel.DtoName}>({resultVar}, {MappingContextVar}, includeTree);");
                }
                else
                {
                    // Return type is primitive or an IClassDto
                    b.Line($"_result.{resultProp} = {resultVar};");
                }
            }

            b.Append("return _result;");
        }
    }
}

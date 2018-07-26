using IntelliTect.Coalesce.CodeGeneration.Generation;
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

        public const string MethodResultVar = "methodResult";

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
            // Don't try to store the result in the variable if the method returns void.
            if (!method.TaskUnwrappedReturnType.IsVoid)
            {
                b.Append($"var {MethodResultVar} = ");
            }

            var awaitSymbol = method.IsAwaitable ? "await " : "";
            b.Line($"{awaitSymbol}{owningMember}.{method.Name}({method.CsArguments});");
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

            if (method.TaskUnwrappedReturnType.IsA<ApiResult>())
            {
                // For any ApiResult return type, pass it to our ApiResult ctor to grab the WasSuccessful and Message props.
                // For list results, this also copies paging information.
                b.Line($"var result = new {method.ApiActionReturnTypeDeclaration}({MethodResultVar});");
            }
            else
            {
                // Return type isn't an ApiResult - just create a 'blank' object.
                b.Line($"var result = new {method.ApiActionReturnTypeDeclaration}();");
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
                    b.Line("var mappingContext = new MappingContext(User, \"\");");

                    // ToList the result (because it might be IQueryable - we need to execute the query before mapping)
                    b.Append($"result.{resultProp} = {resultVar}?.ToList().Select(o => ");
                    b.Append($"Mapper.MapToDto<{resultType.PureType.ClassViewModel.FullyQualifiedName}, {resultType.PureType.ClassViewModel.DtoName}>");

                    // Only attempt to pull the include tree out of the result if the user actually typed their return type as an IQueryable.
                    var includeTreeForMapping = resultType.IsA<IQueryable>()
                        ? $"includeTree ?? ({resultVar} as IQueryable)?.GetIncludeTree()"
                        : "includeTree";

                    b.Append($"(o, mappingContext, {includeTreeForMapping})).ToList();");
                    b.Line();
                }
                else
                {
                    // Return type is a collection of primitives or IClassDtos.
                    // This ToList() may end up being redundant, but it is guaranteed to be safe.
                    // The minimum type required here that handles all cases is IList<T> (required by a ListResult<T> return type).
                    b.Line($"result.{resultProp} = {resultVar}?.ToList();");
                }
            }
            else
            {
                if (resultType.HasClassViewModel && !resultType.ClassViewModel.IsDto)
                {
                    // Return type is a model that needs to be mapped to a DTO.
                    b.Line("var mappingContext = new MappingContext(User, \"\");");
                    b.Line($"result.{resultProp} = Mapper.MapToDto<{resultType.ClassViewModel.FullyQualifiedName}, {resultType.ClassViewModel.DtoName}>({resultVar}, mappingContext, includeTree);");
                }
                else
                {
                    // Return type is primitive or an IClassDto
                    b.Line($"result.{resultProp} = {resultVar};");
                }
            }

            b.Append("return result;");
        }
    }
}

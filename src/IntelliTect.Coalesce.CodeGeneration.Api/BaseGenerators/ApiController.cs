using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators
{
    public abstract class ApiController : RazorTemplateCSharpGenerator<ClassViewModel>
    {
        public ApiController(RazorTemplateServices razorServices) : base(razorServices) { }

        public string MethodResultProcessBlock(MethodViewModel method, int indentLevel = 2)
        {
            var b = new CodeBuilder(initialLevel: indentLevel, indentSize: IndentationSize);

            var resultVar = "methodResult";
            var resultProp = "Object";
            var resultType = method.ResultType;
            if (method.ReturnType.IsA(typeof(ItemResult<>)))
            {
                resultVar = "methodResult.Object";
            }
            else if (method.ReturnsListResult)
            {
                resultProp = "List";
                resultVar = "methodResult.List";
            }

            if (method.ReturnType.IsA<ApiResult>())
            {
                // For any ApiResult return type, pass it to our ApiResult ctor to grab the WasSuccessful and Message props.
                // For list results, this also copies paging information.
                b.Line($"var result = new {method.ReturnTypeNameForApi}(methodResult);");
            }
            else
            {
                b.Line($"var result = new {method.ReturnTypeNameForApi}();");
            }

            if (resultType.IsVoid)
            {
                // If the result type is void, there's no methodResult to stick on our return object.
            }
            else if (resultType.IsA<ApiResult>())
            {
                // If the resultType (not the ReturnType) is an API result, this means that the method is either:
                // 2) Returning exactly ApiResult and not a derived type of it. In this case, we can safely ignore because there is no Object on it to process.
                // 3) Returning an ItemResult<T> or ListResult<T> where T : ApiResult. This is bonkers. Don't do this.
            }
            else if (resultType.IsCollection)
            {
                if (resultType.PureType.HasClassViewModel && !resultType.PureType.ClassViewModel.IsDto)
                {
                    // Return type is a collection of models that need to be mapped to a DTO.
                    b.Line("var mappingContext = new MappingContext(User, \"\");");

                    // Only call ToList if the result isn't a collection (might be IEnumerable or IQueryable)
                    b.Append($"result.{resultProp} = {resultVar}.ToList().Select(o => ");
                    b.Append($"Mapper.MapToDto<{resultType.PureType.ClassViewModel.FullyQualifiedName}, {resultType.PureType.ClassViewModel.DtoName}>");

                    // Only attempt to pull the include tree out of the result if the user actually typed their return type as an IQueryable.
                    var pullIncludeTree = resultType.IsA<IQueryable>() ? $"includeTree ?? ({resultVar} as IQueryable)?.GetIncludeTree()" : "includeTree";
                    b.Append($"(o, mappingContext, {pullIncludeTree})).ToList();");
                    b.Line();
                }
                else
                {
                    // Return type is a collection of primitives or IClassDtos.
                    // This ToList() may end up being redundant, but it is guaranteed to be safe.
                    // The minimum type required here that handles all cases is IList<T> (required by a ListResult<T> return type).
                    b.Line($"result.{resultProp} = {resultVar}.ToList();");
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

            return b.ToString();
        }
    }
}

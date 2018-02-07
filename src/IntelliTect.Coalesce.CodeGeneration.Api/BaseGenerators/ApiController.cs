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


            // Unwrap an ItemResult<T> and process its .Object as if it were the actual return type of the method.
            var resultVar = "methodResult";
            var resultType = method.ResultType;
            if (method.ReturnType.IsA(typeof(ItemResult<>)))
            {
                resultVar = "unwrappedResult";
                b.Line("var unwrappedResult = methodResult.Object;");
            }

            if (method.ReturnType.IsA<ApiResult>())
            {
                // For any ApiResult return type, pass it to our ItemResult<T> ctor to grab the WasSuccessful and Message props.
                b.Line($"var result = new ItemResult<{method.ReturnTypeNameForApi}>(methodResult);");
            }
            else
            {
                b.Line($"var result = new ItemResult<{method.ReturnTypeNameForApi}>();");
            }

            if (resultType.IsA<ApiResult>())
            {
                // If the unwrapped result type is an API result, this means that the method is either:
                // 1) Returning a derivative of ApiResult that we don't currently support unwrapping. This basically only includes ListResult<>.
                // 2) Returning exactly ApiResult and not a derived type of it. In this case, we can safely ignore because there is no Object on it to process.
                // 3) Returning an ItemResult<T> where T : ApiResult. This is bonkers. Don't do this.
            }
            else if (resultType.IsCollection)
            {
                if (resultType.PureType.ClassViewModel?.IsDto ?? false)
                {
                    b.Line($"result.Object = {resultVar}.ToList();");
                }
                else if (resultType.PureType.HasClassViewModel)
                {
                    b.Line("var mappingContext = new MappingContext(User, \"\");");
                    b.Line($"result.Object = {resultVar}.ToList().Select(o => Mapper.MapToDto <{resultType.PureType.ClassViewModel.FullyQualifiedName}, {resultType.PureType.ClassViewModel.DtoName}>(o, mappingContext, ({resultVar} as IQueryable)?.GetIncludeTree() ?? includeTree)).ToList();");
                }
                else
                {
                    b.Line($"result.Object = {resultVar};");
                }
            }
            else
            {
                if (resultType.ClassViewModel?.IsDto ?? false)
                {
                    b.Line($"result.Object = {resultVar};");
                }
                else if (resultType.HasClassViewModel)
                {
                    b.Line("var mappingContext = new MappingContext(User, \"\");");
                    b.Line($"result.Object = Mapper.MapToDto<{resultType.ClassViewModel.FullyQualifiedName}, {resultType.ClassViewModel.DtoName}>({resultVar}, mappingContext, includeTree);");
                }
                else
                {
                    b.Line($"result.Object = {resultVar};");
                }
            }

            b.Append("return result;");

            return b.ToString();
        }
    }
}

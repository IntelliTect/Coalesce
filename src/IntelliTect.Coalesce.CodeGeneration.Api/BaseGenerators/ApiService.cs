using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;

public abstract class ApiService : StringBuilderCSharpGenerator<ClassViewModel>
{
    public ApiService(GeneratorServices services) : base(services)
    {
    }

    public const string MethodResultVar = "_methodResult";
    public const string MappingContextVar = "_mappingContext";

    protected string GetAreaNamespace()
    {
        string namespaceName = Namespace;
        if (!string.IsNullOrWhiteSpace(AreaName))
        {
            namespaceName += "." + AreaName;
        }
        return namespaceName;
    }

    /// <summary>
    /// For a method invocation controller action, writes the actual invocation of the method.
    /// </summary>
    /// <param name="b">The CodeBuilder to write to</param>
    /// <param name="method">The method to be invoked</param>
    /// <param name="owningMember">
    /// The member on which to invoke the method. 
    /// This could be an variable holding an instance of a type, or a class reference if the method is static.
    /// </param>
    public void WriteMethodInvocation(
        CSharpCodeBuilder b,
        MethodViewModel method,
        string owningMember
    )
    {
        var clientParameters = method.ClientParameters.ToList();
        if (clientParameters.Count > 0)
        {
            var validateAttributes = method.GetAttributeValue<ExecuteAttribute, bool>(
                e => e.ValidateAttributes,
                e => e.ValidateAttributesHasValue);
            if (validateAttributes == true)
            {
                WriteMethodValidation(b, method);
            }
            else if (validateAttributes == null)
            {
                using (b.Block("if (Context.Options.ValidateAttributesForMethods)"))
                {
                    WriteMethodValidation(b, method);
                }
            }
            b.Line();
        }

        if (method.ResultType.PureType.ClassViewModel?.IsCustomDto == false)
        {
            b.Line("IncludeTree includeTree = null;");
        }

        if (method.Parameters.Any(p => !p.IsDI && p.PureType.HasClassViewModel)
            || method.ResultType.PureType.HasClassViewModel
        )
        {
            b.Line($"var {MappingContextVar} = new {nameof(MappingContext)}(Context);");
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
                b.Line(); // Uncomment for parameter-per-line
                b.Append(GetMethodInvocationParameterExpression(parameters[i]));
            }
        }
        if (parameters.Any()) b.Line(); // Uncomment for parameter-per-line
        b.Line(");");
    }

    protected void WriteMethodViewModelVar(CSharpCodeBuilder b, MethodViewModel method)
    {
        b.Line($"var _method = GeneratedForClassViewModel!.MethodByName({method.Name.QuotedStringLiteralForCSharp()});");
    }

    private void WriteMethodValidation(CSharpCodeBuilder b, MethodViewModel method)
    {
        // You may be wondering... why don't we just check ModelState?
        // Well, the main reason is that the generated DTOs (which are the parameters at least for complex types):
        // 1) have all nullable properties and so any inherent required-ness of non-nullable properties is lost
        // 2) model attributes are not copied from models to generated DTOs, so any attributes aren't validated by ModelState.
        // 3) validation for the /save endpoint is done in Behaviors and is therefore outside/beyond when ModelState is available. Validating methods like this is therefore consistent with SaveImplementation.

        // One thing that this code does NOT cover, that could be covered by ModelState validation,
        // would be actual deserialization issues that cause property values to get skipped.
        // However, when interacting with Coalesce through the generated TS, this should never be a problem
        // and so is not a high priority to add checking of ModelState for this reason.

        b.Line("var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);");

        if (method.ApiActionReturnTypeDeclaration == "ItemResult")
        {
            b.Line("if (!_validationResult.WasSuccessful) return _validationResult;");
        }
        else
        {
            b.Line($"if (!_validationResult.WasSuccessful) return new {method.ApiActionReturnTypeDeclaration}(_validationResult);");
        }
    }


    /// <summary>
    /// For a method invocation controller action, builds an expression for the given parameter
    /// that maps the controller input to the argument of method being called.
    /// </summary>
    protected string GetMethodInvocationParameterExpression(ParameterViewModel param)
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

        return $"_params.{param.PascalCaseName}{param.MapToModelChain(MappingContextVar)}";
    }

    protected void WriteInstanceMethodTargetLoading(CSharpCodeBuilder b, MethodViewModel method)
    {
        b.Line($"var dataSource = dataSourceFactory.GetDataSource<" +
            $"{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>" +
            $"(\"{method.LoadFromDataSourceName}\");");

        if (Model.IsCustomDto)
        {
            b.Line($"var itemResult = await dataSource.GetMappedItemAsync<{Model.FullyQualifiedName}>(_params.Id, new DataSourceParameters());");
        }
        else
        {
            b.Line("var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());");
        }
        using (b.Block("if (!itemResult.WasSuccessful)"))
        {
            b.Line($"return new {method.ApiActionReturnTypeDeclaration}(itemResult);");
        }
        b.Line("var item = itemResult.Object;");
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
                var contentStreamVar = $"{resultVar}.{nameof(IFile.Content)}";
                b.Line($"return File({resultVar});");
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

                // Stop generating, since all generated code paths have returned,
                // so we don't generate unreachable code.
                return;
            }
        }


        var includeTreeForMapping = "includeTree";
        if (method.TaskUnwrappedReturnType.IsA<ApiResult>())
        {
            includeTreeForMapping += $" ?? {MethodResultVar}.IncludeTree";

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
            if (resultType.PureType.HasClassViewModel && !resultType.PureType.ClassViewModel.IsCustomDto)
            {
                // Return type is a collection of models that need to be mapped to a DTO.
                // ToList the result (because it might be IQueryable - we need to execute the query before mapping)
                b.Append($"_result.{resultProp} = {resultVar}?.ToList().Select(o => ");
                b.Append($"Mapper.MapToDto<{resultType.PureType.ClassViewModel.FullyQualifiedName}, {resultType.PureType.ClassViewModel.ResponseDtoTypeName}>");

                // Only attempt to pull the include tree out of the result if the user actually typed their return type as an IQueryable.
                if (resultType.IsA<IQueryable>())
                {
                    includeTreeForMapping += $" ?? ({resultVar} as IQueryable)?.GetIncludeTree()";
                }

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
            if (resultType.HasClassViewModel && !resultType.ClassViewModel.IsCustomDto)
            {
                // Return type is a model that needs to be mapped to a DTO.
                b.Line($"_result.{resultProp} = Mapper.MapToDto<{resultType.ClassViewModel.FullyQualifiedName}, {resultType.ClassViewModel.ResponseDtoTypeName}>({resultVar}, {MappingContextVar}, {includeTreeForMapping});");
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

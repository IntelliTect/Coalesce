using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators;

#nullable enable

public record ParameterInfo(string TypeName, string Name, string? Description = null, string? DefaultValue = null);

public class KernelPlugin(GeneratorServices services) : ApiService(services)
{
    public override void BuildOutput(CSharpCodeBuilder b)
    {
        var namespaceName = GetAreaNamespace();

        var namespaces = new List<string>
        {
            "IntelliTect.Coalesce",
            "IntelliTect.Coalesce.Api",
            "IntelliTect.Coalesce.Api.Behaviors",
            "IntelliTect.Coalesce.Api.DataSources",
            "IntelliTect.Coalesce.Mapping",
            "IntelliTect.Coalesce.Models",
            "IntelliTect.Coalesce.TypeDefinition",
            "Microsoft.SemanticKernel",
            "System",
            "System.ComponentModel",
            "System.Linq",
            "System.Collections.Generic",
            "System.Security.Claims",
            "System.Threading.Tasks",
            // This is the output namespace for the generated DTOs
            $"{namespaceName}.Models"
        };
        foreach (var ns in namespaces.OrderBy(n => n))
        {
            b.Line($"using {ns};");
        }

        b.Line();
        b.Line($"namespace {namespaceName}.KernelPlugins;");
        b.Line("#pragma warning disable CS1998"); // Async method lacks 'await' operators and will run synchronously
        b.Line();

        var dbContext = Model.DbContext;
        var crudCtxType = dbContext is not null ? $"CrudContext<{dbContext.FullyQualifiedName}>" : "CrudContext";

        List<string> injectedServices = [
            $"{crudCtxType} context",
        ];

        if (Model.IsService)
        {
            injectedServices.Add($"{Model.Type.FullyQualifiedName} _service");
        }
        else
        {
            injectedServices.Add($"IDataSourceFactory dataSourceFactory");
        }

        if (Model.GetAttribute<SemanticKernelAttribute>() is { } ska && (
            ska.GetValue(a => a.SaveEnabled) == true ||
            ska.GetValue(a => a.DeleteEnabled) == true
        ))
        {
            injectedServices.Add("IBehaviorsFactory behaviorsFactory");
        }

        // Individual method dependencies must be ctor injected so that InvokedScoped
        // can acquire new instances of those services.
        foreach (var param in Model.KernelMethods
            .SelectMany(method => method.Parameters)
            .Where(p => p.ShouldInjectFromServices)
        )
        {
            injectedServices.Add($"{param.Type.FullyQualifiedName} {GetDiParameterName(param)}");
        }
        injectedServices = injectedServices.Distinct().ToList();

        using var _ = b.Block($"public class {Model.Name}KernelPlugin({string.Join(", ", injectedServices)}) : KernelPluginBase<{Model.FullyQualifiedName}>(context)");

        if (dbContext is not null) b.Line($"protected {dbContext.FullyQualifiedName} Db => context.DbContext;");

        WriteSaveFunction(b);
        WriteDeleteFunction(b);

        var dataSources = Model.ClientDataSources(Model.ReflectionRepository!).Where(ds => ds.HasAttribute<SemanticKernelAttribute>()).ToList();
        var hasMultipleDataSources = dataSources.Count > 1;

        if (
            Model.GetAttribute<SemanticKernelAttribute>()?.GetValue(a => a.DefaultDataSourceEnabled) == true &&
            !dataSources.Any(ds => ds.IsDefaultDataSource)
        )
        {
            hasMultipleDataSources |= dataSources.Count == 1;
            WriteDataSourceGetItemFunction(b, null, hasMultipleDataSources);
            WriteDataSourceListFunction(b, null, hasMultipleDataSources);
        }

        foreach (var ds in dataSources)
        {
            WriteDataSourceGetItemFunction(b, ds, hasMultipleDataSources);
            WriteDataSourceListFunction(b, ds, hasMultipleDataSources);
        }

        foreach (var method in Model.KernelMethods)
        {
            WriteMethodFunction(b, method);
        }
    }

    private void WriteMethodFunction(CSharpCodeBuilder b, MethodViewModel method)
    {
        if (method.ResultType.IsFile)
        {
            throw new NotSupportedException("File result types are not supported in kernel plugins.");
        }

        List<ParameterInfo> parameters = [];
        foreach (var param in method.Parameters
            .Where(p => p.IsDI && !p.IsNonArgumentDI && !p.ShouldInjectFromServices)
            .Concat(method.ApiParameters)
            .OrderBy(p => p.HasDefaultValue))
        {
            string typeName;
            if (param.PureType.IsFile)
            {
                throw new NotSupportedException("File parameters are not supported in kernel plugins.");
            }
            else if (param.IsDI)
            {
                typeName = param.Type.FullyQualifiedName;
            }
            else
            {
                typeName = param.Type.NullableTypeForDto(isInput: true, dtoNamespace: null, dontEmitNullable: true);
            }

            var paramDesc = param.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
            var defaultValue = param.HasDefaultValue ? param.CsDefaultValue : null;
            var paramInfo = new ParameterInfo(typeName, param.CsParameterName, paramDesc, defaultValue);
            parameters.Add(paramInfo);
        }

        var description = method.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
        using var _ = WriteKernelMethodSignature(b, method.Name, description, parameters);

        WriteMethodViewModelVar(b, method);
        b.Line($"if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new {method.ApiActionReturnTypeDeclaration}(errorMessage: \"Unauthorized\");");

        var clientParameters = method.ApiParameters.ToList();
        if (clientParameters.Count > 0)
        {
            using (b.Block("var _params = new", ";"))
            {
                for (int i = 0; i < clientParameters.Count; i++)
                {
                    var param = clientParameters[i];
                    if (i != 0) b.Line(", ");
                    b.Append(param.PascalCaseName);
                    b.Append(" = ");
                    b.Append(param.CsParameterName);
                }
            }
        }

        foreach (var diParam in method.Parameters.Where(p => p.ShouldInjectFromServices))
        {
            // Map parameters that were injected into the SK plugin class
            b.Line($"var {diParam.CsParameterName} = {GetDiParameterName(diParam)};");
        }
        b.Line();

        if (method.IsStatic)
        {
            WriteMethodInvocation(b, method, method.Parent.FullyQualifiedName);
        }
        else if (method.EffectiveParent.IsService)
        {
            WriteMethodInvocation(b, method, "_service");
        }
        else
        {
            WriteInstanceMethodTargetLoading(b, method);
            WriteMethodInvocation(b, method, "item");
        }

        WriteMethodResultProcessBlock(b, method);
    }

    // future: add flags to KPA to enable/disable specific actions (read, list, ...?)

    private void WriteDataSourceGetItemFunction(CSharpCodeBuilder b, ClassViewModel? ds, bool hasMultipleDataSources)
    {
        string? description;
        string dsName;
        List<PropertyViewModel> dsParameters;

        if (ds is null)
        {
            description = Model.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
            dsName = DataSourceFactory.DefaultSourceName;
            dsParameters = [];
        }
        else
        {
            description = ds.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
            dsName = ds.Name;
            dsParameters = ds.DataSourceParameters.ToList();
        }

        var pkVar = Model.PrimaryKey!.JsonName;
        var parameters = new List<ParameterInfo>
        {
            new ParameterInfo(Model.PrimaryKey.Type.FullyQualifiedName, pkVar)
        };

        var methodName = hasMultipleDataSources ? $"Get{Model.Name}{dsName}" : $"Get{Model.Name}";

        var descriptionAttrPart = string.IsNullOrWhiteSpace(description) ? "" : " " + description.TrimEnd('.', ' ') + ".";
        using var _ = WriteKernelMethodSignature(b, methodName,
            $"Gets a {Model.DisplayName} by its {Model.PrimaryKey.Name} value.{descriptionAttrPart}", parameters);

        b.Line("if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return \"Unauthorized.\";");
        b.Line();

        if (ds is null)
        {
            b.Line($"var _dataSource = dataSourceFactory.GetDefaultDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>();");
        }
        else
        {
            b.Line($"var _dataSource = dataSourceFactory.GetDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>({dsName.QuotedStringLiteralForCSharp()});");
        }

        b.Line($"var _dataSourceParams = new DataSourceParameters {{ DataSource = {dsName.QuotedStringLiteralForCSharp()}}};");
        b.Line($"return await _dataSource.GetMappedItemAsync<{Model.ResponseDtoTypeName}>({pkVar}, _dataSourceParams);");
    }

    private void WriteDataSourceListFunction(CSharpCodeBuilder b, ClassViewModel? ds, bool hasMultipleDataSources)
    {
        string? description;
        string dsName;
        List<PropertyViewModel> dsParameters;

        if (ds is null)
        {
            description = Model.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
            dsName = DataSourceFactory.DefaultSourceName;
            dsParameters = [];
        }
        else
        {
            description = ds.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
            dsName = ds.Name;
            dsParameters = ds.DataSourceParameters.ToList();
        }

        var dataSourceParameters = dsParameters.Select(p =>
        {
            string? desc = p.GetAttributeValue<SemanticKernelAttribute>(a => a.Description) ?? p.Description;
            return new ParameterInfo(
                p.Type.NullableTypeForDto(true, null, dontEmitNullable: true),
                p.JsonName,
                desc,
                "default"
            );
        }).ToList();

        var allParameters = new List<ParameterInfo>
        {
            new("string", "search", "Search within properties " + string.Join(",", Model.SearchProperties().Select(p => p.Property.Name))),
            new("int", "page", "Provide values greater than 1 to query subsequent pages of data"),
            new("bool", "countOnly", "Provide true if you only need a count of results."),
            new("string[]", "fields", "Leave empty if you need whole objects, or provide any of these field names to trim the response: " + string.Join(",", Model.ClientProperties.Select(p => p.Name)))
        };
        allParameters.AddRange(dataSourceParameters);

        var methodName = hasMultipleDataSources ? $"List{Model.Name}{dsName}" : $"List{Model.Name}";
        var resultType = $"ListResult<{Model.ResponseDtoTypeName}>";

        var descriptionAttrPart = string.IsNullOrWhiteSpace(description) ? "" : " " + description.TrimEnd('.', ' ') + ".";
        using var _ = WriteKernelMethodSignature(b, methodName,
            $"Lists {Model.DisplayName} records.{descriptionAttrPart}", allParameters);

        b.Line($"if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new {resultType}(errorMessage: \"Unauthorized.\");");
        b.Line();

        if (ds is null)
        {
            b.Line($"var _dataSource = dataSourceFactory.GetDefaultDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>();");
        }
        else
        {
            b.Line($"var _dataSource = ({ds.Type.FullyQualifiedName})dataSourceFactory.GetDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>({dsName.QuotedStringLiteralForCSharp()});");
        }
        b.Line("MappingContext _mappingContext = new(context);");
        foreach (var param in dsParameters)
        {
            b.Line($"_dataSource.{param.Name} = {param.JsonName}{param.MapToModelChain("_mappingContext")};");
        }
        b.Line();

        if (dsParameters.Any())
        {
            b.Line("if (ItemResult.FromValidation(_dataSource) is { WasSuccessful: false } _validationResult)");
            b.Indented($"return new {resultType}(_validationResult);");
            b.Line();
        }

        b.Line($"var _listParams = new ListParameters {{ DataSource = {dsName.QuotedStringLiteralForCSharp()}, Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 25 }};");
        using (b.Block("if (countOnly)"))
        {
            b.Line("var result = await _dataSource.GetCountAsync(_listParams);");
            b.Line($"return new {resultType}(result) {{ TotalCount = result.Object }};");
        }

        b.Line($"return await _dataSource.GetMappedListAsync<{Model.ResponseDtoTypeName}>(_listParams);");
    }

    private void WriteSaveFunction(CSharpCodeBuilder b)
    {
        if (!Model.SecurityInfo.IsSaveAllowed()) return;

        var kpa = Model.GetAttribute<SemanticKernelAttribute>();
        if (kpa?.GetValue(a => a.SaveEnabled) != true) return;

        var isSparse = !Model.IsCustomDto || Model.Type.IsA<ISparseDto>();

        List<string> descriptionParts = [];
        if (Model.SecurityInfo.IsCreateAllowed()) descriptionParts.Add($"Creates a new {Model.DisplayName}");
        if (Model.SecurityInfo.IsEditAllowed()) descriptionParts.Add($"Updates an existing {Model.DisplayName}");
        descriptionParts = [string.Join(" or ", descriptionParts) + "."];

        var parameters = new List<ParameterInfo>
        {
            new ParameterInfo(Model.ParameterDtoTypeName, "dto", isSparse
                ? "The values to update. Only provide value of the fields that need to be changed."
                : "Provide the value of all fields, even those that are not being changed.")
        };

        using var _ = WriteKernelMethodSignature(b, $"Save{Model.Name}",
            string.Concat(descriptionParts), parameters);

        WriteDefaultDataSourceAndBehaviors(b);

        b.Lines(
            "var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;",
            "if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))",
            $"    return \"Creation of {Model.DisplayName} items not allowed.\";",
            "if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))",
            $"    return \"Editing of {Model.DisplayName} items not allowed.\";"
        );

        b.Line($"return await behaviors.SaveAsync<{Model.ParameterDtoTypeName}, {Model.ResponseDtoTypeName}>(dto, dataSource, new DataSourceParameters());");
    }

    private void WriteDeleteFunction(CSharpCodeBuilder b)
    {
        if (!Model.SecurityInfo.IsDeleteAllowed()) return;

        var kpa = Model.GetAttribute<SemanticKernelAttribute>();
        if (kpa?.GetValue(a => a.DeleteEnabled) != true) return;

        var pkVar = Model.PrimaryKey!.JsonName;
        var parameters = new List<ParameterInfo>
        {
            new(Model.PrimaryKey.Type.FullyQualifiedName, pkVar)
        };

        using var _ = WriteKernelMethodSignature(b, $"Delete{Model.Name}",
            $"Deletes an existing {Model.DisplayName}.", parameters);

        WriteDefaultDataSourceAndBehaviors(b);

        b.Lines(
            "if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))",
            $"    return \"Deleting of {Model.DisplayName} items not allowed.\";"
        );

        b.Line($"return await behaviors.DeleteAsync<{Model.ResponseDtoTypeName}>({pkVar}, dataSource, new DataSourceParameters());");
    }

    private static string GetDiParameterName(ParameterViewModel param)
    {
        return param.Type.Name.GetValidCSharpIdentifier();
    }

    protected IDisposable WriteKernelMethodSignature(CSharpCodeBuilder b, string methodName, string? description, List<ParameterInfo> parameters)
    {
        var functionName = methodName.ToSnakeCase();

        b.Line();
        b.Line($"[KernelFunction(\"{functionName}\")]");
        if (description is not null) b.Line($"[Description({description.QuotedStringLiteralForCSharp()})]");

        b.Append($"public async Task<string> {methodName}(");

        using (b.Indented())
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                b.Line();
                var paramInfo = parameters[i];

                if (paramInfo.Description is not null)
                {
                    b.Line($"[Description({paramInfo.Description.QuotedStringLiteralForCSharp()})]");
                }

                b.Append(paramInfo.TypeName);
                b.Append(" ");
                b.Append(paramInfo.Name);

                if (paramInfo.DefaultValue is not null)
                {
                    b.Append(" = ");
                    b.Append(paramInfo.DefaultValue);
                }

                b.Append(i < parameters.Count - 1 ? "," : "");
            }
        }

        var methodBlock = b.Block(")");

        var invokeParams = string.Concat(parameters.Select(p => ", " + p.Name));
        b.Line($"if (!_isScoped) return await InvokeScoped<string>({methodName}{invokeParams});");
        b.Line();

        // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
        var jsonBlock = b.Block("return await Json(async () => ", ");");

        return new CompositeDisposable(methodBlock, jsonBlock);
    }

    private void WriteDefaultDataSourceAndBehaviors(CSharpCodeBuilder b)
    {
        b.Line($"var dataSource = dataSourceFactory.GetDefaultDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>();");
        b.Line($"var behaviors = behaviorsFactory.GetBehaviors<{Model.BaseViewModel.FullyQualifiedName}>(GeneratedForClassViewModel);");
        b.Line();
    }

    private class CompositeDisposable(params IDisposable[] disposables) : IDisposable
    {
        public void Dispose()
        {
            foreach (var disposable in disposables.Reverse())
            {
                disposable?.Dispose();
            }
        }
    }
}

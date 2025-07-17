using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators;

public class KernelPlugin(GeneratorServices services) : ApiService(services)
{
    protected string FullNamespace
    {
        get
        {
            string namespaceName = Namespace;
            if (!string.IsNullOrWhiteSpace(AreaName))
            {
                namespaceName += "." + AreaName;
            }
            namespaceName += ".KernelPlugins";
            return namespaceName;
        }
    }

    public override void BuildOutput(CSharpCodeBuilder b)
    {
        string namespaceName = Namespace;
        if (!string.IsNullOrWhiteSpace(AreaName))
        {
            namespaceName += "." + AreaName;
        }

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
            $"IDataSourceFactory dsFactory",
            $"IBehaviorsFactory bhFactory",
        ];

        if (Model.IsService)
        {
            injectedServices.Add($"{Model.Type.FullyQualifiedName} _service");
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
        b.Line();

        WriteSaveFunction(b);
        WriteDeleteFunction(b);

        foreach (var ds in Model.ClientDataSources(Model.ReflectionRepository).Where(ds => ds.HasAttribute<SemanticKernelAttribute>()))
        {
            WriteDataSourceGetItemFunction(b, ds);
            WriteDataSourceListFunction(b, ds);
        }

        foreach (var method in Model.KernelMethods)
        {
            WriteMethodFunction(b, method);
        }
    }

    private static string GetDiParameterName(ParameterViewModel param)
    {
        return param.Type.Name.GetValidCSharpIdentifier();
    }

    protected List<string> WriteMethodFunctionSignature(CSharpCodeBuilder b, MethodViewModel method)
    {
        //b.Append($"public async Task<{method.ApiActionReturnTypeDeclaration}> {method.Name}");
        b.Append($"public async Task<string> {method.Name}");

        b.Line("(");
        using var indent = b.Indented();

        List<string> paramNames = [];
        foreach (var param in method.Parameters
            .Where(f => f.IsDI && !f.IsNonArgumentDI)
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

            if (param.ShouldInjectFromServices)
            {
                // DI injections are always pulled from ctor injections
                continue;
            }

            var description = param.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);
            if (description is not null)
            {
                b.Line($"[Description({description.QuotedStringLiteralForCSharp()})]");
            }

            paramNames.Add(param.CsParameterName);
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

        return paramNames;
    }

    private void WriteMethodFunction(CSharpCodeBuilder b, MethodViewModel method)
    {
        var description = method.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);

        if (method.ResultType.IsFile)
        {
            throw new NotSupportedException("File result types are not supported in kernel plugins.");
        }

        b.Line($"[KernelFunction(\"{method.NameWithoutAsync}\")]");
        b.Line($"[Description({description.QuotedStringLiteralForCSharp()})]");
        var paramNames = WriteMethodFunctionSignature(b, method);
        using (b.Block())
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>({method.Name}{string.Concat(paramNames.Select(p => ", " + p))});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");

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
                b.Line($"var dataSource = dsFactory.GetDataSource<" +
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

                WriteMethodInvocation(b, method, "item");
            }

            WriteMethodResultProcessBlock(b, method);
        }
        b.Line();
    }

    // todo: add a disambiguator string to KPA that we add to the function name, in case a single type has multiple exposed DS's.
    // todo: add flags to KPA to enable/disable specific actions (read, list, ...?)


    private void WriteDataSourceGetItemFunction(CSharpCodeBuilder b, ClassViewModel ds)
    {
        var description = ds.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);

        var pkVar = Model.PrimaryKey.JsonName;
        var declaredFor = Model.FullyQualifiedName;
        var dsNameString = ds.Name.QuotedStringLiteralForCSharp();


        b.Line($"[KernelFunction(\"get_{Model.Name.ToLower()}\")]");
        b.Line($"[Description(\"Gets a {Model.DisplayName} by its {Model.PrimaryKey.Name} value. {description}.\")]");

        var resultType = $"ItemResult<{Model.ResponseDtoTypeName}>";
        using (b.Block($"public async Task<string> Get{Model.Name}({Model.PrimaryKey.Type.FullyQualifiedName} {pkVar})"))
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>(Get{Model.Name}, {pkVar});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");

            b.Line("if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return \"Unauthorized.\";");
            b.Line();

            b.Line($"var dataSource = dsFactory.GetDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>({dsNameString});");
            b.Line($"var dataSourceParams = new DataSourceParameters {{ DataSource = {dsNameString}}};");
            b.Line($"return await dataSource.GetMappedItemAsync<{Model.ResponseDtoTypeName}>({pkVar}, dataSourceParams);");
        }
        b.Line();
    }

    private void WriteDataSourceListFunction(CSharpCodeBuilder b, ClassViewModel ds)
    {
        var description = ds.GetAttributeValue<SemanticKernelAttribute>(kp => kp.Description);

        var pkVar = Model.PrimaryKey.JsonName;
        var declaredFor = Model.FullyQualifiedName;
        var dsNameString = ds.Name.QuotedStringLiteralForCSharp();


        var resultType = $"ListResult<{Model.ResponseDtoTypeName}>";
        string dataSourceParams = string.Concat(ds.DataSourceParameters.Select(p =>
        {
            string ret = ", \n";
            string desc =
                p.GetAttributeValue<SemanticKernelAttribute>(a => a.Description)
                ?? p.Description;

            if (!string.IsNullOrWhiteSpace(desc))
            {
                ret += $"        [Description({desc.QuotedStringLiteralForCSharp()})]\n";
            }

            ret += $"        {p.Type.NullableTypeForDto(true, null, dontEmitNullable: true)} {p.JsonName} = default";

            return ret;
        }));

        b.Line($"[KernelFunction(\"list_{Model.Name.ToLower()}\")]");
        b.Line($"[Description(\"Lists {Model.DisplayName} records. {description}.\")]");

        using (b.Block($"""
                public async Task<string> List{Model.Name}(
                    [Description("Search within properties {string.Join(",", Model.SearchProperties().Select(p => p.Property.Name))}")]
                    string search, 
                    [Description("Provide values greater than 1 to query subsequent pages of data")]
                    int page,
                    [Description("Provide true if you only need a count of results.")]
                    bool countOnly,
                    [Description("Leave empty if you need whole objects, or provide any of these field names to trim the response: {string.Join(",", Model.ClientProperties.Select(p => p.Name))}")]
                    string[] fields{dataSourceParams}
                )
            """))
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>(List{Model.Name}, search, page, countOnly, fields{string.Concat(ds.DataSourceParameters.Select(p => ", " + p.JsonName))});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");


            b.Line($"if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(User)) return new {resultType}(errorMessage: \"Unauthorized.\");");
            b.Line();

            b.Line($"var dataSource = ({ds.Type.FullyQualifiedName})dsFactory.GetDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>({dsNameString});");
            b.Line("MappingContext mappingContext = new(context);");
            foreach (var param in ds.DataSourceParameters)
            {
                b.Line($"dataSource.{param.Name} = {param.JsonName}{param.MapToModelChain("mappingContext")};");
            }
            b.Line();

            b.Line($"var listParams = new ListParameters {{ DataSource = {dsNameString}, Search = search, Page = page, Fields = string.Join(',', fields), PageSize = 100 }};");
            using (b.Block("if (countOnly)"))
            {
                b.Line("var result = await dataSource.GetCountAsync(listParams);");
                b.Line($"return new {resultType}(result) {{ TotalCount = result.Object }};");
            }
            b.Line($"return await dataSource.GetMappedListAsync<{Model.ResponseDtoTypeName}>(listParams);");
        }
        b.Line();
    }

    private void WriteSaveFunction(CSharpCodeBuilder b)
    {
        if (!Model.SecurityInfo.IsSaveAllowed()) return;

        var kpa = Model.GetAttribute<SemanticKernelAttribute>();
        if (kpa?.GetValue(a => a.SaveEnabled) != true) return;

        var pkVar = Model.PrimaryKey.JsonName;
        var declaredFor = Model.FullyQualifiedName;
        var isSparse = !Model.IsCustomDto || Model.Type.IsA<ISparseDto>();


        var resultType = $"ItemResult<{Model.ResponseDtoTypeName}>";

        b.Line($"[KernelFunction(\"save_{Model.Name.ToLower()}\")]");

        List<string> descriptionParts = [];
        if (Model.SecurityInfo.IsCreateAllowed()) descriptionParts.Add($"Creates a new {Model.DisplayName}");
        if (Model.SecurityInfo.IsEditAllowed()) descriptionParts.Add($"Updates an existing {Model.DisplayName}");
        descriptionParts = [string.Join(" or ", descriptionParts) + "."];

        b.Line($"[Description({string.Concat(descriptionParts).QuotedStringLiteralForCSharp()})]");
        using (b.Block($"""
            public async Task<string> Save{Model.Name}(
                [Description({(isSparse
                    ? "The values to update. Only provide value of the fields that need to be changed."
                    : "Provide the value of all fields, even those that are not being changed."
                ).QuotedStringLiteralForCSharp()})]
                {Model.ParameterDtoTypeName} dto
            )
        """))
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>(Save{Model.Name}, dto);");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");

            b.Line($"var dataSource = dsFactory.GetDefaultDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>();");
            b.Line($"var behaviors = bhFactory.GetBehaviors<{Model.BaseViewModel.FullyQualifiedName}>(GeneratedForClassViewModel);");

            b.Lines(
                "var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, new DataSourceParameters())).Kind;",
                "if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))",
                $"    return \"Creation of {Model.DisplayName} items not allowed.\";",
                "if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))",
                $"    return \"Editing of {Model.DisplayName} items not allowed.\";"
            );

            b.Line($"return await behaviors.SaveAsync<{Model.ParameterDtoTypeName}, {Model.ResponseDtoTypeName}>(dto, dataSource, new DataSourceParameters());");
        }
        b.Line();
    }

    private void WriteDeleteFunction(CSharpCodeBuilder b)
    {
        if (!Model.SecurityInfo.IsDeleteAllowed()) return;

        var kpa = Model.GetAttribute<SemanticKernelAttribute>();
        if (kpa?.GetValue(a => a.DeleteEnabled) != true) return;

        var pkVar = Model.PrimaryKey.JsonName;
        var declaredFor = Model.FullyQualifiedName;

        var resultType = $"ItemResult<{Model.ResponseDtoTypeName}>";

        b.Line($"[KernelFunction(\"delete_{Model.Name.ToLower()}\")]");
        b.Line($"[Description(\"Deletes an existing {Model.DisplayName}.\")]");
        using (b.Block($"""
            public async Task<string> Delete{Model.Name}({Model.PrimaryKey.Type.FullyQualifiedName} {pkVar})
            """))
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>(Delete{Model.Name}, {pkVar});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");

            b.Line($"var dataSource = dsFactory.GetDefaultDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>();");
            b.Line($"var behaviors = bhFactory.GetBehaviors<{Model.BaseViewModel.FullyQualifiedName}>(GeneratedForClassViewModel);");

            b.Lines(
                "if (!GeneratedForClassViewModel.SecurityInfo.IsDeleteAllowed(User))",
                $"    return \"Deleting of {Model.DisplayName} items not allowed.\";"
            );

            b.Line($"return await behaviors.DeleteAsync<{Model.ResponseDtoTypeName}>({pkVar}, dataSource, new DataSourceParameters());");
        }
        b.Line();
    }
}

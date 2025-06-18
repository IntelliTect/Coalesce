using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators;

public class KernelPlugins : CompositeGenerator<ReflectionRepository>
{
    public KernelPlugins(CompositeGeneratorServices services) : base(services) { }

    public override IEnumerable<ICleaner> GetCleaners()
    {
        yield return Cleaner<DirectoryCleaner>()
            .AppendTargetPath("KernelPlugins/Generated");
    }

    public override IEnumerable<IGenerator> GetGenerators()
    {
        foreach (var model in Model.CrudApiBackedClasses)
        {
            if (model.WillCreateApiController && (
                    model.SecurityInfo.IsReadAllowed() ||
                    model.SecurityInfo.IsSaveAllowed() ||
                    model.SecurityInfo.IsDeleteAllowed()
                ) &&
                model.ClientDataSources(Model).Any(ds => ds.HasAttribute<KernelPluginAttribute>())
            )
            {
                yield return Generator<KernelPlugin>()
                    .WithModel(model)
                    .AppendOutputPath($"KernelPlugins/Generated/{model.ClientTypeName}KernelPlugin.g.cs");
            }
        }
    }
}

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
        ];

        // Individual method dependencies must be ctor injected so that InvokedScoped
        // can acquire new instances of those services.
        foreach (var param in Model.ClientMethods
            .Where(ds => ds.HasAttribute<KernelPluginAttribute>())
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

        foreach (var ds in Model.ClientDataSources(Model.ReflectionRepository).Where(ds => ds.HasAttribute<KernelPluginAttribute>()))
        {
            WriteDataSourceGetItemFunction(b, ds);
            WriteDataSourceListFunction(b, ds);
        }

        foreach (var method in Model.ClientMethods.Where(ds => ds.HasAttribute<KernelPluginAttribute>()))
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
        var description = method.GetAttributeValue<KernelPluginAttribute>(kp => kp.Description);

        var pkVar = Model.PrimaryKey.JsonName;

        if (method.ResultType.IsFile)
        {
            throw new NotSupportedException("File result types are not supported in kernel plugins.");
        }

        // todo: add a disambiguator string to KPA that we add to the function name, in case a single type has multiple exposed DS's.
        // todo: add flags to KPA to enable/disable specific actions (read, list, ...?)

        b.Line($"[KernelFunction(\"{method.NameWithoutAsync}\")]");
        b.Line($"[Description({description.QuotedStringLiteralForCSharp()})]");
        var paramNames = WriteMethodFunctionSignature(b, method);
        using (b.Block())
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>({method.Name}{string.Concat(paramNames.Select(p => ", " + p))});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");

            WriteMethodViewModelVar(b, method);
            b.Line($"if (!_method.SecurityInfo.IsExecuteAllowed(context.User)) return new {method.ApiActionReturnTypeDeclaration}(errorMessage: \"Unauthorized\");");

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

    private void WriteDataSourceGetItemFunction(CSharpCodeBuilder b, ClassViewModel ds)
    {
        var description = ds.GetAttributeValue<KernelPluginAttribute>(kp => kp.Description);

        var pkVar = Model.PrimaryKey.JsonName;
        var declaredFor = Model.FullyQualifiedName;
        var dsNameString = ds.Name.QuotedStringLiteralForCSharp();

        // todo: add a disambiguator string to KPA that we add to the function name, in case a single type has multiple exposed DS's.
        // todo: add flags to KPA to enable/disable specific actions (read, list, ...?)

        b.Line($"[KernelFunction(\"get_{Model.Name.ToLower()}\")]");
        b.Line($"[Description(\"Gets a {Model.Name} by its {Model.PrimaryKey.Name} value. {description}.\")]");

        var resultType = $"ItemResult<{Model.ResponseDtoTypeName}>";
        using (b.Block($"public async Task<string> Get{Model.Name}({Model.PrimaryKey.Type.FullyQualifiedName} {pkVar})"))
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string>(Get{Model.Name}, {pkVar});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");

            b.Line("if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(context.User)) return \"Unauthorized.\";");
            b.Line();

            b.Line($"var dataSource = dsFactory.GetDataSource<{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>({dsNameString});");
            b.Line($"var dataSourceParams = new DataSourceParameters {{ DataSource = {dsNameString}}};");
            b.Line($"return await dataSource.GetMappedItemAsync<{Model.ResponseDtoTypeName}>({pkVar}, dataSourceParams);");
        }
        b.Line();
    }

    private void WriteDataSourceListFunction(CSharpCodeBuilder b, ClassViewModel ds)
    {
        var description = ds.GetAttributeValue<KernelPluginAttribute>(kp => kp.Description);

        var pkVar = Model.PrimaryKey.JsonName;
        var declaredFor = Model.FullyQualifiedName;
        var dsNameString = ds.Name.QuotedStringLiteralForCSharp();

        // todo: add a disambiguator string to KPA that we add to the function name, in case a single type has multiple exposed DS's.
        // todo: add flags to KPA to enable/disable specific actions (read, list, ...?)

        var resultType = $"ListResult<{Model.ResponseDtoTypeName}>";

        b.Line($"[KernelFunction(\"list_{Model.Name.ToLower()}\")]");
        b.Line($"[Description(\"Lists {Model.Name} records. {description}. The search parameter can search on properties {string.Join(",", Model.SearchProperties().Select(p => p.Property.Name))}. The fields parameter should be used if you only need some of the following fields: {string.Join(",", Model.ClientProperties.Select(p => p.Name))}\")]");
        using (b.Block($"""
            public async Task<string> List{Model.Name}(
                    string search, 
                    int page,
                    bool countOnly,
                    string[] fields
                    {string.Concat(ds.DataSourceParameters.Select(p => ", " + p.Type.NullableTypeForDto(true, null, dontEmitNullable: true) + " " + p.JsonName + " = default"))}
                )
            """))
        {
            b.Line($"if (!_isScoped) return await InvokeScoped<string> (List{Model.Name}, search, page, countOnly, fields{string.Concat(ds.DataSourceParameters.Select(p => ", " + p.JsonName))});");

            // Workaround for https://github.com/microsoft/semantic-kernel/issues/12532
            using var json = b.Block("return await Json(async () => ", ");");


            b.Line($"if (!GeneratedForClassViewModel.SecurityInfo.IsReadAllowed(context.User)) return new {resultType}(errorMessage: \"Unauthorized.\");");
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
}

#if NET9_0_OR_GREATER
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.OpenApi;

internal class CoalesceApiOperationFilter : IOpenApiOperationTransformer
{
    private readonly ReflectionRepository reflectionRepository;
    private readonly ILookup<(string? HttpMethod, string? RelativePath), ApiDescription> descriptionsByEndpoint;

    public CoalesceApiOperationFilter(
        IApiDescriptionGroupCollectionProvider apiDescriptions,
        ReflectionRepository reflectionRepository
    )
    {
        this.reflectionRepository = reflectionRepository;

        descriptionsByEndpoint = apiDescriptions.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .ToLookup(d => (d.HttpMethod, d.RelativePath));
    }

    public Task TransformAsync(
        OpenApiOperation operation, 
        OpenApiOperationTransformerContext context, 
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor cad ||
            !cad.ControllerTypeInfo.IsAssignableTo(typeof(BaseApiController)))
        {
            return Task.CompletedTask;
        }

        var methodInfo = cad.MethodInfo;
        var cvm = reflectionRepository.GetClassViewModel(methodInfo.DeclaringType!)!;
        var method = new ReflectionMethodViewModel(methodInfo, cvm, cvm);

        AddOtherBodyTypes(operation, context);
        ProcessDataSources(operation, context, method);
        ProcessStandardParameters(operation, method);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Workaround https://github.com/dotnet/aspnetcore/issues/58329
    /// </summary>
    private async void AddOtherBodyTypes(OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        Type? docServiceType = Type.GetType("Microsoft.AspNetCore.OpenApi.OpenApiDocumentService,Microsoft.AspNetCore.OpenApi");
        if (docServiceType is null) return;

        var description = context.Description;

        var otherDescriptions = descriptionsByEndpoint[(description.HttpMethod, description.RelativePath)]
            .Where(d => d != description);

        foreach (var otherDescription in otherDescriptions)
        {
            object docService = context.ApplicationServices.GetRequiredKeyedService(docServiceType, context.DocumentName);
            var resultTask = docServiceType
                .GetMethod("GetRequestBodyAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                .Invoke(docService, [
                    // ApiDescription description,
                    otherDescription,
                    // IServiceProvider scopedServiceProvider,
                    context.ApplicationServices,
                    // IOpenApiSchemaTransformer[] schemaTransformers,
                    // TODO: Too hard to acquire schema transformers here.
                    Array.Empty<IOpenApiSchemaTransformer>(),
                    // CancellationToken cancellationToken
                    CancellationToken.None
                ]) as Task<OpenApiRequestBody>;

            if (resultTask is null) continue;

            var result = await resultTask;
            if (result is null) continue;

            foreach (var otherContent in result.Content)
            {
                operation.RequestBody.Content.Add(otherContent);
            }
        }
    }

    private void ProcessStandardParameters(OpenApiOperation operation, MethodViewModel method)
    {
        foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA<IDataSourceParameters>()))
        {
            var paramType = paramVm.Type.ClassViewModel!;

            // Join our ParameterViewModels with the OpenApiParameters.
            var paramsUnion = paramType.ClientProperties.Join(
                operation.Parameters, p => p.Name, p => p.Name, (pvm, nbp) => (PropViewModel: pvm, OperationParam: nbp)
            );

            foreach (var noSetterProp in paramsUnion.Where(p =>
                // Remove "Filter" - we'll enumerate all available filter params
                (p.PropViewModel.Name == nameof(IFilterParameters.Filter) && p.OperationParam.Schema.Type == "object")
            ))
            {
                operation.Parameters.Remove(noSetterProp.OperationParam);
            }

            if (paramVm.Type.IsA<IFilterParameters>())
            {
                // Kind of a hacky way to get this, but we don't have a whole lot of other context here.
                var modelType = method.Parent.Type
                    .GenericArgumentsFor(typeof(IntelliTect.Coalesce.Api.Controllers.BaseApiController<,,>))
                    ?[0];

                if (modelType?.ClassViewModel is ClassViewModel cvm)
                {
                    foreach (var filterProp in cvm.ClientProperties.Where(p => p.IsUrlFilterParameter))
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            In = ParameterLocation.Query,
                            Name = $"filter.{filterProp.Name}",
                            Required = false,
                            Description = $"Filters results by values contained in property '{filterProp.JsonName}'.",
                            Schema = new OpenApiSchema { Type = "string" }
                        });
                    }
                }
            }
        }

        if (method.ResultType.IsA<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>>())
        {
            operation.Responses["200"].Content.Clear();
            operation.Responses["200"].Content["application/octet-stream"] = new OpenApiMediaType
            {
                Schema = new() { Type = "string", Format = "binary" }
            };
        }
    }

    private void ProcessDataSources(OpenApiOperation operation, OpenApiOperationTransformerContext context, MethodViewModel method)
    {
        var iDataSourceParam = method.Parameters.FirstOrDefault(p => p.Type.IsA(typeof(IDataSource<>)));
        if (iDataSourceParam is null) return;

        var declaredFor =
            iDataSourceParam.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)
            ?? iDataSourceParam.Type.GenericArgumentsFor(typeof(IDataSource<>))!.Single();

        var dataSources = declaredFor.ClassViewModel!.ClientDataSources(reflectionRepository);

        var dataSourceNameParam = operation.Parameters?.FirstOrDefault(p => p.Name == nameof(IDataSourceParameters.DataSource));
        if (dataSourceNameParam is not null)
        {
            dataSourceNameParam.Schema = new OpenApiSchema
            {
                Type = "string",
                Enum = (new string[] { IntelliTect.Coalesce.Api.DataSources.DataSourceFactory.DefaultSourceName })
                    .Concat(dataSources.Select(ds => ds.ClientTypeName))
                    .Select(n => new OpenApiString(n) as IOpenApiAny)
                    .ToList()
            };

            foreach (var param in dataSources.SelectMany(ds => ds.DataSourceParameters).GroupBy(ds => ds.Name))
            {
                var openApiParam = operation.Parameters?.FirstOrDefault(p => 
                    p.Name.Equals($"{dataSourceNameParam.Name}.{param.Key}", StringComparison.OrdinalIgnoreCase));

                if (openApiParam is not null)
                {
                    var dataSourceNames = string.Join(", ", param.Select(p => p.EffectiveParent.ClientTypeName));

                    openApiParam.Description = string.Join(". \n", (new List<string?>(param.Select(p => p.Description))
                    {
                        $"Used by data source{(param.Count() == 1 ? "" : "s")} {dataSourceNames}."
                    }).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
                }
            }
        }
    }
}
#endif

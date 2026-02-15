#if NET9_0_OR_GREATER
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
using System.Text.Json.Nodes;
#else
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
#endif
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

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor cad ||
            !cad.ControllerTypeInfo.IsAssignableTo(typeof(BaseApiController)))
        {
            return;
        }

        var methodInfo = cad.MethodInfo;
        var cvm = reflectionRepository.GetClassViewModel(methodInfo.DeclaringType!)!;
        var method = new ReflectionMethodViewModel(methodInfo, cvm, cvm);

        await AddOtherBodyTypes(operation, context, cancellationToken);
        ProcessDataSources(operation, context, method);
        ProcessStandardParameters(operation, method);
    }

    /// <summary>
    /// Workaround https://github.com/dotnet/aspnetcore/issues/58329
    /// </summary>
    private async Task AddOtherBodyTypes(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
    {
        Type? docServiceType = Type.GetType("Microsoft.AspNetCore.OpenApi.OpenApiDocumentService,Microsoft.AspNetCore.OpenApi");
        if (docServiceType is null) return;

        var description = context.Description;

        var otherDescriptions = descriptionsByEndpoint[(description.HttpMethod, description.RelativePath)]
            .Where(d => d != description);

        foreach (var otherDescription in otherDescriptions)
        {
            object docService = context.ApplicationServices.GetRequiredKeyedService(docServiceType, context.DocumentName);
#if NET10_0_OR_GREATER
            // In .NET 10, GetRequestBodyAsync requires OpenApiDocument as the first parameter
            var resultTask = docServiceType
                .GetMethod("GetRequestBodyAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                .Invoke(docService, [
                    // OpenApiDocument document,
                    context.Document,
                    // ApiDescription description,
                    otherDescription,
                    // IServiceProvider scopedServiceProvider,
                    context.ApplicationServices,
                    // IOpenApiSchemaTransformer[] schemaTransformers,
                    // TODO: Too hard to acquire schema transformers here.
                    Array.Empty<IOpenApiSchemaTransformer>(),
                    // CancellationToken cancellationToken
                    ct
                ]) as Task<OpenApiRequestBody>;
#else
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
                    ct
                ]) as Task<OpenApiRequestBody>;
#endif

            if (resultTask is null) continue;

            var result = await resultTask;
            if (result?.Content is null) continue;

            if (operation.RequestBody?.Content is not null)
            {
                foreach (var otherContent in result.Content)
                {
                    operation.RequestBody.Content.Add(otherContent);
                }
            }
        }
    }

    private void ProcessStandardParameters(OpenApiOperation operation, MethodViewModel method)
    {
        foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA<IDataSourceParameters>()))
        {
            var paramType = paramVm.Type.ClassViewModel!;

            // Join our ParameterViewModels with the OpenApiParameters.
            if (operation.Parameters != null)
            {
                var paramsUnion = paramType.ClientProperties.Join(
                    operation.Parameters, p => p.Name, p => p.Name, (pvm, nbp) => (PropViewModel: pvm, OperationParam: nbp)
                );

                foreach (var noSetterProp in paramsUnion.Where(p =>
                    // Remove "Filter" - we'll enumerate all available filter params
                    p.PropViewModel.Name == nameof(IFilterParameters.Filter)
#if NET10_0_OR_GREATER
                    && p.OperationParam.Schema?.Type == JsonSchemaType.Object
#else
                    && p.OperationParam.Schema?.Type == "object"
#endif
                ))
                {
                    operation.Parameters.Remove(noSetterProp.OperationParam);
                }
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
                        operation.Parameters ??= [];
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            In = ParameterLocation.Query,
                            Name = $"filter.{filterProp.Name}",
                            Required = false,
                            Description = $"Filters results by values contained in property '{filterProp.JsonName}'.",
                            Schema = new OpenApiSchema
                            {
#if NET10_0_OR_GREATER
                                Type = JsonSchemaType.String,
#else
                                Type = "string",
#endif    
                            }
                        });
                    }
                }
            }
        }

        if (method.ResultType.IsA<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>>())
        {
            operation.Responses!["200"].Content!.Clear();
            operation.Responses["200"].Content!["application/octet-stream"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema()
                {
#if NET10_0_OR_GREATER
                    Type = JsonSchemaType.String,
#else
                    Type = "string",
#endif    
                    Format = "binary"
                }
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
        if (dataSourceNameParam is not null && operation.Parameters is not null)
        {
            var enumValues = (new string[] { IntelliTect.Coalesce.Api.DataSources.DataSourceFactory.DefaultSourceName })
                .Concat(dataSources.Select(ds => ds.ClientTypeName));

#if NET10_0_OR_GREATER
            // In OpenAPI.NET 2.0, Schema is read-only, so we need to create a new parameter
            var newSchema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = enumValues.Select(n => JsonValue.Create(n) as JsonNode).ToList()
            };

            var newParam = new OpenApiParameter
            {
                Name = dataSourceNameParam.Name,
                In = dataSourceNameParam.In,
                Description = dataSourceNameParam.Description,
                Required = dataSourceNameParam.Required,
                Schema = newSchema
            };

            var index = operation.Parameters.IndexOf(dataSourceNameParam);
            operation.Parameters.RemoveAt(index);
            operation.Parameters.Insert(index, newParam);
#else
            dataSourceNameParam.Schema = new OpenApiSchema
            {
                Type = "string",
                Enum = enumValues
                    .Select(n => new OpenApiString(n) as IOpenApiAny)
                    .ToList()
            };
#endif

            foreach (var param in dataSources.SelectMany(ds => ds.DataSourceParameters).GroupBy(ds => ds.Name))
            {
                var openApiParam = operation.Parameters.FirstOrDefault(p =>
                    p.Name?.Equals($"{dataSourceNameParam.Name}.{param.Key}", StringComparison.OrdinalIgnoreCase) == true);

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

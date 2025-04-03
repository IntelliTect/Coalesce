using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Swashbuckle
{
    public class CoalesceApiOperationFilter : IOperationFilter
    {
        private readonly ReflectionRepository reflectionRepository;
        private readonly IServiceProvider serviceProvider;
        private readonly ILookup<(string HttpMethod, string RelativePath), ApiDescription> descriptionsByEndpoint;

        public CoalesceApiOperationFilter(
            ReflectionRepository reflectionRepository,
            IApiDescriptionGroupCollectionProvider apiDescriptions,
            IServiceProvider serviceProvider
        )
        {
            this.reflectionRepository = reflectionRepository;
            this.serviceProvider = serviceProvider;

            descriptionsByEndpoint = apiDescriptions.ApiDescriptionGroups.Items
                .SelectMany(g => g.Items)
                .ToLookup(d => (d.HttpMethod, d.RelativePath));
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var cvm = reflectionRepository.GetClassViewModel(context.MethodInfo.DeclaringType);
            var method = new ReflectionMethodViewModel(context.MethodInfo, cvm, cvm);

            AddOtherBodyTypes(operation, context);
            ProcessDataSources(operation, context, method);
            ProcessStandardParameters(operation, method);
        }

        /// <summary>
        /// Workaround https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2270
        /// </summary>
        private void AddOtherBodyTypes(OpenApiOperation operation, OperationFilterContext context)
        {
            var generator = serviceProvider.GetRequiredService<ISwaggerProvider>();
            var description = context.ApiDescription;

            var otherDescriptions = descriptionsByEndpoint[(description.HttpMethod, description.RelativePath)]
                .Where(d => d != description);

            foreach (var otherDescription in otherDescriptions)
            {
                var otherBody = generator
                    .GetType()
                    .GetMethod("GenerateRequestBody", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(generator, [otherDescription, context.SchemaRepository]) as OpenApiRequestBody;

                if (otherBody is null) continue;

                // To mirror legacy behavior before JSON-accepting endpoints were added to Coalesce,
                // only add the "multipart/form-data" body, but not the urlencoded body.
                foreach (var otherContent in otherBody.Content.Where(c => 
                    c.Key is "multipart/form-data" or "application/json"
                ))
                {
                    operation.RequestBody.Content.Add(otherContent);
                }
            }
        }

        private void ProcessStandardParameters(OpenApiOperation operation, MethodViewModel method)
        {
            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA<IDataSourceParameters>()))
            {
                var paramType = paramVm.Type.ClassViewModel;

                // Join our ParameterViewModels with the Swashbuckle IParameters.
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

                    if (modelType != null)
                    {
                        foreach (var filterProp in modelType.ClassViewModel.ClientProperties.Where(p => p.IsUrlFilterParameter))
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

        private void ProcessDataSources(OpenApiOperation operation, OperationFilterContext context, MethodViewModel method)
        {
            var iDataSourceParam = method.Parameters.FirstOrDefault(p => p.Type.IsA(typeof(IDataSource<>)));
            if (iDataSourceParam is null) return;

            var declaredFor =
                iDataSourceParam.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)
                ?? iDataSourceParam.Type.GenericArgumentsFor(typeof(IDataSource<>))!.Single();

            var dataSources = declaredFor.ClassViewModel!.ClientDataSources(reflectionRepository);

            var dataSourceNameParam = operation.Parameters.FirstOrDefault(p => p.Name == nameof(IDataSourceParameters.DataSource));
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
                    var openApiParam = operation.Parameters.FirstOrDefault(p =>
                        p.Name.Equals($"{dataSourceNameParam.Name}.{param.Key}", StringComparison.OrdinalIgnoreCase));

                    if (openApiParam is not null)
                    {
                        var dataSourceNames = string.Join(", ", param.Select(p => p.EffectiveParent.ClientTypeName));

                        openApiParam.Description = string.Join(". \n", (new List<string>(param.Select(p => p.Description))
                        {
                            $"Used by data source{(param.Count() == 1 ? "" : "s")} {dataSourceNames}."
                        }).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
                    }
                }
            }
        }
    }
}

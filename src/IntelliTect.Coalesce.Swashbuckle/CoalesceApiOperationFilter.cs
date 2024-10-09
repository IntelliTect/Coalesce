using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace IntelliTect.Coalesce.Swashbuckle
{
    public class CoalesceApiOperationFilter : IOperationFilter
    {
        public CoalesceApiOperationFilter(ReflectionRepository reflectionRepository)
        {
            ReflectionRepository = reflectionRepository;
        }

        public ReflectionRepository ReflectionRepository { get; }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var cvm = ReflectionRepository.GetClassViewModel(context.MethodInfo.DeclaringType);
            var method = new ReflectionMethodViewModel(cvm, context.MethodInfo);

            ProcessDataSources(operation, context, method);
            ProcessStandardParameters(operation, method);
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
            // In all reality, there will only ever be one data source parameter per action,
            // but might as well not make assumptions if we don't have to.
            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA(typeof(IDataSource<>))))
            {
                var declaredFor =
                    paramVm.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)
                    ?? paramVm.Type.GenericArgumentsFor(typeof(IDataSource<>))!.Single();

                var dataSources = declaredFor.ClassViewModel.ClientDataSources(ReflectionRepository);

                var dataSourceParam = new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = paramVm.Name,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = (new string[] { IntelliTect.Coalesce.Api.DataSources.DataSourceFactory.DefaultSourceName })
                        .Concat(dataSources.Select(ds => ds.ClientTypeName))
                        .Select(n => new OpenApiString(n) as IOpenApiAny)
                        .ToList()
                    },
                };
                operation.Parameters.Add(dataSourceParam);

                foreach (var dataSource in dataSources)
                {
                    foreach (var dsParam in dataSource.DataSourceParameters)
                    {
                        var schema = context.SchemaGenerator.GenerateSchema(dsParam.Type.TypeInfo, context.SchemaRepository);

                        operation.Parameters.Add(new OpenApiParameter
                        {
                            In = ParameterLocation.Query,
                            Name = $"{paramVm.Name}.{dsParam.Name}",
                            Required = false,
                            Description = $"Used by Data Source {dataSource.ClientTypeName}",
                            Schema = schema,
                        });
                    }
                }
            }
        }
    }
}

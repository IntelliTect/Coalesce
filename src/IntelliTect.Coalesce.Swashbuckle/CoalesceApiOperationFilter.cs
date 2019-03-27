using IntelliTect.Coalesce.TypeDefinition;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
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

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var cvm = ReflectionRepository.GetClassViewModel(context.MethodInfo.DeclaringType);
            var method = new ReflectionMethodViewModel(cvm, context.MethodInfo);

            ProcessDataSources(operation, context, method);
            ProcessStandardParameters(operation, method);
        }

        private void ProcessStandardParameters(Operation operation, MethodViewModel method)
        {

            // Remove behaviors - behaviors accept no input from the client.
            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA(typeof(IBehaviors<>))))
            {
                operation.Parameters.Remove(operation.Parameters.Single(p => p.Name == paramVm.Name));
            }

            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA<IDataSourceParameters>()))
            {
                var paramType = paramVm.Type.ClassViewModel;

                // Join our ParameterViewModels with the Swashbuckle IParameters.
                var paramsUnion = paramType.ClientProperties.Join(
                    operation.Parameters.OfType<NonBodyParameter>(), p => p.Name, p => p.Name, (pvm, nbp) => (PropViewModel: pvm, OperationParam: nbp)
                );

                foreach (var noSetterProp in paramsUnion.Where(p =>
                    // Remove params that have no setter. 
                    // Honestly I'm clueless why this isn't default behavior, but OK.
                    !p.PropViewModel.HasSetter

                    // Remove the string "DataSource" parameter that is redundant with our data source model binding functionality.
                    || (p.PropViewModel.Name == nameof(IDataSourceParameters.DataSource) && p.OperationParam.Type == "string")
                    // Remove "Filter" - we'll enumerate all available filter params (for lack of a better solution with OpenAPI 2.0
                    || (p.PropViewModel.Name == nameof(IFilterParameters.Filter) && p.OperationParam.Type == "object")
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
                            operation.Parameters.Add(new NonBodyParameter
                            {
                                In = "query",
                                Name = $"filter.{filterProp.Name}",
                                Required = false,
                                Description = $"Filters results by values contained in property '{filterProp.JsonName}'.",
                                Type = "string",
                            });
                        }
                    }
                }
            }
        }

        private void ProcessDataSources(Operation operation, OperationFilterContext context, MethodViewModel method)
        {
            // In all reality, there will only ever be one data source parameter per action,
            // but might as well not make assumptions if we don't have to.
            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA(typeof(IDataSource<>))))
            {
                var dataSourceParam = operation.Parameters
                    .OfType<NonBodyParameter>()
                    .Single(p => p.Name == paramVm.Name && p.Type == "object");

                var declaredFor =
                    paramVm.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)
                    ?? paramVm.Type.GenericArgumentsFor(typeof(IDataSource<>)).Single();

                var dataSources = declaredFor.ClassViewModel.ClientDataSources(ReflectionRepository);

                dataSourceParam.Type = "string";
                dataSourceParam.Enum =
                    (new object[] { IntelliTect.Coalesce.Api.DataSources.DataSourceFactory.DefaultSourceName })
                    .Concat(dataSources.Select(ds => ds.ClientTypeName as object))
                    .ToList();

                foreach (var dataSource in dataSources)
                {
                    foreach (var dsParam in dataSources.SelectMany(s => s.DataSourceParameters))
                    {
                        var schema = context.SchemaRegistry.GetOrRegister(dsParam.Type.TypeInfo);

                        operation.Parameters.Add(new NonBodyParameter
                        {
                            In = "query",
                            Name = $"{paramVm.Name}.{dsParam.Name}",
                            Required = false,
                            Description = $"Used by Data Source {dataSource.ClientTypeName}",
                            // Data source parameters only support a fairly simple set of params,
                            // and all *should* be representable with just these two values.
                            Type = schema.Type,
                            Format = schema.Format,
                        });
                    }
                }
            }
        }
    }
}

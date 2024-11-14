using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace IntelliTect.Coalesce.Api
{
    /// <summary>
    /// Performs adjustments to the API metadata so that it doesn't cause .NET 9's OpenAPI generation to implode.
    /// In particular, we have to remove the parameters that are bound with Coalesce's custom model binders,
    /// since these cause the OpenAPI generator to throw exceptions. We re-add these definitions with
    /// CoalesceApiOperationFilter.
    /// </summary>
    internal class CoalesceApiDescriptionProvider(ReflectionRepository reflectionRepository) : IApiDescriptionProvider
    {
        public int Order => 0;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var operation in context.Results)
            {
                if (operation.ActionDescriptor is not ControllerActionDescriptor cad) continue;

                var methodInfo = cad.MethodInfo;
                var cvm = reflectionRepository.GetClassViewModel(methodInfo.DeclaringType!)!;
                var method = new ReflectionMethodViewModel(cvm, methodInfo);

                ProcessStandardParameters(operation, method);
            }
        }

        private void ProcessStandardParameters(ApiDescription operation, MethodViewModel method)
        {
            var parameters = operation.ParameterDescriptions;

            // Remove crud strategy parameters, which are bound with a custom model binder
            // and can't meaningfully be represented by the API explorer.
            // We add them back in in CoalesceApiOperationFilter.
            // They break the new Microsoft.AspNetCore.OpenApi package in .NET 9
            // if we leave them present in the API descriptions.
            foreach (var paramVm in method.Parameters.Where(p =>
                p.Type.IsA(typeof(IBehaviors<>)) || 
                p.Type.IsA(typeof(IDataSource<>))
            ))
            {
                parameters.Remove(parameters.Single(p => p.Name == paramVm.Name));
            }

            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA<IDataSourceParameters>()))
            {
                var paramType = paramVm.Type.ClassViewModel!;

                // Join our ParameterViewModels with the ApiParameterDescription parameters.
                var paramsUnion = paramType.ClientProperties.Join(
                    parameters, p => p.Name, p => p.Name, (pvm, nbp) => (PropViewModel: pvm, OperationParam: nbp)
                );

                foreach (var noSetterProp in paramsUnion.Where(p =>
                    // Remove params that have no setter. 
                    // Honestly I'm clueless why this isn't default behavior, but OK.
                    !p.PropViewModel.HasSetter
                ))
                {
                    parameters.Remove(noSetterProp.OperationParam);
                }
            }

            foreach (var paramVm in method.Parameters.Where(p => p.Type.IsA(typeof(IDataSource<>))))
            {
                var declaredFor =
                    paramVm.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)
                    ?? paramVm.Type.GenericArgumentsFor(typeof(IDataSource<>))!.Single();

                var dataSources = declaredFor.ClassViewModel!.ClientDataSources(reflectionRepository);

                foreach (var dataSource in dataSources)
                {
                    foreach (var dsParam in dataSource.DataSourceParameters)
                    {
                        operation.ParameterDescriptions.Add(new ApiParameterDescription()
                        {
                            Source = BindingSource.Query,
                            Name = $"{paramVm.Name}.{dsParam.Name}",
                            IsRequired = false,
                            Type = dsParam.Type.TypeInfo,
                            ModelMetadata = operation.ParameterDescriptions.First().ModelMetadata.GetMetadataForProperty(dataSource.Type.TypeInfo, dsParam.Name)
                        });
                    }
                }
            }
        }
    }
}

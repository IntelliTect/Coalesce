using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Api;

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
            var method = new ReflectionMethodViewModel(methodInfo, cvm, cvm);

            ProcessStandardParameters(operation, method);
#if !NET10_0_OR_GREATER
            FixEnumSerializationType(operation, method);
#endif
        }
    }

#if !NET10_0_OR_GREATER
    /// <summary>
    /// Workaround https://github.com/dotnet/aspnetcore/issues/61327 by correcting enum types from String
    /// to their underlying integral type. Coalesce also just generally only handles enums as numbers on the wire anyway,
    /// so this is also more correctl.
    /// </summary>
    private void FixEnumSerializationType(ApiDescription operation, ReflectionMethodViewModel method)
    {
        var parameters = operation.ParameterDescriptions;
        foreach (var parameter in parameters)
        {
            if (
                parameter.ParameterDescriptor?.ParameterType?.IsAssignableTo(typeof(Enum)) == true &&
                parameter.Type == typeof(string)
            )
            {
                parameter.Type = Enum.GetUnderlyingType(parameter.ParameterDescriptor.ParameterType);
            }
        }
    }
#endif

    private void ProcessStandardParameters(ApiDescription operation, MethodViewModel method)
    {
        var parameters = operation.ParameterDescriptions;

        var standardCrudStrategyParameters = method.Parameters.Where(p =>
            !p.HasAttribute<FromServicesAttribute>() && (
                p.Type.IsA(typeof(IBehaviors<>)) ||
                p.Type.IsA(typeof(IDataSource<>))
            )
        );

        // Remove crud strategy parameters, which are bound with a custom model binder
        // and can't meaningfully be represented by the API explorer.
        // We add them back in in CoalesceApiOperationFilter.
        // They break the new Microsoft.AspNetCore.OpenApi package in .NET 9
        // if we leave them present in the API descriptions.
        foreach (var paramVm in standardCrudStrategyParameters)
        {
            var matchingParam = parameters.SingleOrDefault(p => p.Name == paramVm.Name);
            if (matchingParam is not null)
            {
                parameters.Remove(matchingParam);
            }
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

        foreach (var paramVm in standardCrudStrategyParameters.Where(p => p.Type.IsA(typeof(IDataSource<>))))
        {
            var declaredFor =
                paramVm.GetAttributeValue<DeclaredForAttribute>(a => a.DeclaredFor)
                ?? paramVm.Type.GenericArgumentsFor(typeof(IDataSource<>))!.Single();

            var dataSources = declaredFor.ClassViewModel!.ClientDataSources(reflectionRepository);

            foreach (var group in dataSources.SelectMany(ds => ds.DataSourceParameters).GroupBy(ds => ds.Name))
            {
                var dsParam = group.First();
                var dataSource = dsParam.Parent;
                var type = dsParam.Type.TypeInfo;

                // If the datasource parameter is a class, transform the type into its generated parameter DTO,
                // which is what is actually used at runtime by DataSourceModelBinder.
                if (
                    reflectionRepository.GetOrAddType(type) is TypeViewModel tvm &&
                    tvm.PureType.ClassViewModel is ClassViewModel cvm &&
                    reflectionRepository.GeneratedParameterDtos.GetValueOrDefault(cvm) is ClassViewModel paramDtoType
                )
                {
                    type = paramDtoType.Type.TypeInfo;
                    if (tvm.IsCollection)
                    {
                        type = type.MakeArrayType();
                    }
                }

                operation.ParameterDescriptions.Add(new ApiParameterDescription()
                {
                    Source = BindingSource.Query,
                    Name = $"{paramVm.Name}.{dsParam.Name}",
                    IsRequired = false,
                    Type = type,
                    ModelMetadata = operation.ParameterDescriptions.First().ModelMetadata.GetMetadataForProperty(dataSource.Type.TypeInfo, dsParam.Name)
                });
            }
        }
    }
}

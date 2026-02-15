using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.Api.CrudStrategy;

public abstract class CrudStrategyModelBinder
{
    protected (ClassViewModel ServedType, ClassViewModel DeclaredFor) GetStrategyTypes(
        ModelBindingContext bindingContext,
        Type strategyInterface
    )
    {
        // Grab the type information about what we need to inject.
        var parameterTypeViewModel = ReflectionRepository.Global.GetOrAddType(bindingContext.ModelType);

        // Figure out what type is satisfying the generic parameter of our strategy interface.
        // This is the type that our datasource/Behaviors needs to serve.
        var servedType = parameterTypeViewModel.GenericArgumentsFor(strategyInterface)!.Single().ClassViewModel!;

        // Examine the parameter for any attributes.
        var parameterDescriptor = bindingContext.ActionContext.ActionDescriptor.Parameters
            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor>()
            .Single(p => p.Name == bindingContext.FieldName);
        var parameterInfo = parameterDescriptor.ParameterInfo;

        // See if there is an override for the type that we want to find declared data sources for.
        ClassViewModel declaredFor = servedType;
        var declaredForAttribute = parameterInfo.GetAttribute<DeclaredForAttribute>();
        if (declaredForAttribute != null)
        {
            declaredFor = ReflectionRepository.Global.GetClassViewModel(declaredForAttribute.DeclaredFor)!;
        }

        return (servedType, declaredFor);
    }
}

using IntelliTect.Coalesce.Api.CrudStrategy;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.Behaviors;

public class BehaviorsModelBinder : CrudStrategyModelBinder, IModelBinder
{
    private readonly IBehaviorsFactory behaviorsFactory;
    private readonly IModelBinderFactory modelBinderFactory;

    public BehaviorsModelBinder(
        IBehaviorsFactory behaviorsFactory,
        IModelBinderFactory modelBinderFactory)
    {
        this.behaviorsFactory = behaviorsFactory;
        this.modelBinderFactory = modelBinderFactory;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var (servedType, declaredFor) = GetStrategyTypes(bindingContext, typeof(IBehaviors<>));

        object behaviors = behaviorsFactory.GetBehaviors(servedType, declaredFor);

        // Everything worked out; we have behaviors!
        // Hand back our resulting object, and we're done.
        bindingContext.Result = ModelBindingResult.Success(behaviors);

        // Don't validate our behaviors object - nothing on it is user input.
        bindingContext.ValidationState[behaviors] = new ValidationStateEntry() { SuppressValidation = true };

        return Task.CompletedTask;
    }
}

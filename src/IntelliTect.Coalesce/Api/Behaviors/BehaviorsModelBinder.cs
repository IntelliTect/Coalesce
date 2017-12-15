using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.Behaviors
{
    public class BehaviorsModelBinder : IModelBinder
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

            // Grab the type information about what we need to inject,
            // and make sure that we're really binding to an IBehaviors<>.
            var typeViewModel = new ReflectionTypeViewModel(bindingContext.ModelType);
            if (!typeViewModel.IsA(typeof(IBehaviors<>))) return Task.CompletedTask;

            // Figure out what type is satisfying the generic parameter of IBehaviors<>.
            // This is the type that our Behaviors needs to serve.
            var servedType = typeViewModel.GenericArgumentsFor(typeof(IBehaviors<>)).Single();

            object behaviors = behaviorsFactory.GetBehaviors(servedType.ClassViewModel);

            // Everything worked out; we have behaviors!
            // Hand back our resulting object, and we're done.
            bindingContext.Result = ModelBindingResult.Success(behaviors);

            // Don't validate our behaviors object - nothing on it is user input.
            bindingContext.ValidationState[behaviors] = new ValidationStateEntry() { SuppressValidation = true };

            return Task.CompletedTask;
        }
    }
}

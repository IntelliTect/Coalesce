using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

namespace IntelliTect.Coalesce.Api.Behaviors
{
    public class BehaviorsModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            var typeViewModel = new ReflectionTypeViewModel(context.Metadata.ModelType);
            if (!typeViewModel.IsA(typeof(IBehaviors<>))) return null;

            return new BinderTypeModelBinder(typeof(BehaviorsModelBinder));
        }
    }
}

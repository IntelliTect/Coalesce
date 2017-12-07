using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

namespace IntelliTect.Coalesce.Mvc
{
    public class DataSourceModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            var typeViewModel = new ReflectionTypeViewModel(context.Metadata.ModelType);
            if (!typeViewModel.IsA(typeof(IDataSource<>))) return null;

            return new BinderTypeModelBinder(typeof(DataSourceModelBinder));
        }
    }
}

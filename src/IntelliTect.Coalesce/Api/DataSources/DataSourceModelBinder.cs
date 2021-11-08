using IntelliTect.Coalesce.Api.CrudStrategy;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.Api.DataSources
{
    public class DataSourceModelBinder : CrudStrategyModelBinder, IModelBinder
    {
        private readonly IDataSourceFactory dataSourceFactory;
        private readonly IModelBinderFactory modelBinderFactory;

        public DataSourceModelBinder(
            IDataSourceFactory dataSourceFactory,
            IModelBinderFactory modelBinderFactory)
        {
            this.dataSourceFactory = dataSourceFactory;
            this.modelBinderFactory = modelBinderFactory;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Specify a default argument name if none is set by ModelBinderAttribute
            // This is the name of the query parameter on the URL.
            if (string.IsNullOrEmpty(bindingContext.BinderModelName))
            {
                bindingContext.BinderModelName = bindingContext.ModelName;
            }

            var valueProviderResult =
                bindingContext.ValueProvider.GetValue(bindingContext.BinderModelName);

            // This is the name of the dataSource that has been requested.
            var requestedDataSource = valueProviderResult.FirstValue;

            var (servedType, declaredFor) = GetStrategyTypes(bindingContext, typeof(IDataSource<>));

            object dataSource;
            try
            {
                dataSource = dataSourceFactory.GetDataSource(servedType, declaredFor, requestedDataSource);
            }
            catch (DataSourceNotFoundException ex)
            {
                // A data-source that doesn't exist was requested. Add a binding error and quit.
                // The ApiController's IActionFilter (or individual actions) are responsible for handling the response for this error condition.
                bindingContext.ModelState.TryAddModelError(bindingContext.BinderModelName, ex.Message);
                return;
            }

            // We now have an IDataSource<> instance. Get its actual type so we can reflect on it.
            var dataSourceType = dataSource.GetType();


            // From our concrete dataSource, figure out which properties on it are injectable parameters.
            var desiredPropertyViewModels = 
                new ReflectionTypeViewModel(dataSourceType).ClassViewModel!.DataSourceParameters;

            // Get the ASP.NET MVC metadata objects for these properties.
            var desiredPropertiesMetadata = desiredPropertyViewModels
                .Select(propViewModel => bindingContext.ModelMetadata.GetMetadataForProperty(dataSourceType, propViewModel.Name))
                .ToList();

            // Tell the validation stage that it should only perform validation 
            // on the specific properties which we are binding to (and not ALL properties on the dataSource).
            bindingContext.ValidationState[dataSource] = new ValidationStateEntry()
            {
                Strategy = new SelectivePropertyComplexObjectValidationStrategy(desiredPropertiesMetadata)
            };

#pragma warning disable CS0618 // Type or member is obsolete:
            // Will keep using until ComplexTypeModelBinder is fully gone, 
            // in order to maintain compat with all targeted .NET versions.
            var childBinder = new ComplexTypeModelBinder(desiredPropertiesMetadata.ToDictionary(
                property => property,
                property => modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
                {
                    BindingInfo = new BindingInfo()
                    {
                        BinderModelName = property.BinderModelName,
                        BinderType = property.BinderType,
                        BindingSource = property.BindingSource,
                        PropertyFilterProvider = property.PropertyFilterProvider,
                    },
                    Metadata = property,
                    CacheToken = property,
                })
            ), new LoggerFactory() );
#pragma warning restore CS0618 // Type or member is obsolete

            // Enter a nested scope for binding the properties on our dataSource
            // (we're now 1 level deep instead of 0 levels deep).
            using (bindingContext.EnterNestedScope(
                bindingContext.ModelMetadata.GetMetadataForType(dataSourceType),
                bindingContext.FieldName,
                bindingContext.ModelName,
                dataSource))
            {
                bindingContext.PropertyFilter = p => desiredPropertiesMetadata.Contains(p);

                // We call the private method "BindModelCoreAsync" here because
                // "BindModelAsync" performs a check to see if we should bother instantiating the root model (our dataSource).
                // We already instantiated our dataSource, so this check is meaningless. The consequence of this frivolous check is that 
                // it causeses validation of client parameter properties
                // to not occurr if the client didn't provide any values for those parameters.

                // The alternative to do this would be to make a full copy of ComplexTypeModelBinder.cs and
                // change out the desired pieces.
                var bindModelCoreAsync = childBinder
                    .GetType()
                    .GetMethod("BindModelCoreAsync", BindingFlags.NonPublic | BindingFlags.Instance);

#nullable disable warnings
                if (bindModelCoreAsync.GetParameters().Length == 2)
                {
                    // aspnetcore 3+
                    await (bindModelCoreAsync.Invoke(childBinder, new object[] { bindingContext, /*GreedyPropertiesMayHaveData*/ 1 }) as Task);
                }
                else
                {
                    // aspnetcore 2
                    await (bindModelCoreAsync.Invoke(childBinder, new[] { bindingContext }) as Task);
                }
#nullable restore warnings

            }

            // Everything worked out; we have a dataSource!
            // Hand back our resulting object, and we're done.
            bindingContext.Result = ModelBindingResult.Success(dataSource);
        }

        private class SelectivePropertyComplexObjectValidationStrategy : IValidationStrategy
        {
            private readonly ICollection<ModelMetadata> properties;

            public SelectivePropertyComplexObjectValidationStrategy(ICollection<ModelMetadata> properties)
            {
                this.properties = properties;
            }


            /// <inheritdoc />
            public IEnumerator<ValidationEntry> GetChildren(
                ModelMetadata metadata,
                string key,
                object model)
            {
                if (model == null) return Enumerable.Empty<ValidationEntry>().GetEnumerator();

                return properties
                    .Select(p => new ValidationEntry(p, ModelNames.CreatePropertyModelName(key, p.BinderModelName ?? p.PropertyName), model))
                    .GetEnumerator();
            }
        }
    }
}

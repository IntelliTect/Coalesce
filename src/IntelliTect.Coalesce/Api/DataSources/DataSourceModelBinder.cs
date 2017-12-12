using IntelliTect.Coalesce.Api.DataSources;
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

namespace IntelliTect.Coalesce.Api.DataSources
{
    public class DataSourceModelBinder : IModelBinder
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
            if (string.IsNullOrEmpty(bindingContext.BinderModelName))
            {
                bindingContext.BinderModelName = "dataSource";
            }

            var valueProviderResult =
                bindingContext.ValueProvider.GetValue(bindingContext.BinderModelName);

            var requestedDataSource = valueProviderResult.FirstValue;


            var typeViewModel = new ReflectionTypeViewModel(bindingContext.ModelType);
            if (!typeViewModel.IsA(typeof(IDataSource<>))) return;

            var servedType = typeViewModel.GenericArgumentsFor(typeof(IDataSource<>)).Single();

            object dataSource;
            try
            {
                dataSource = dataSourceFactory.GetDataSource(servedType.ClassViewModel, requestedDataSource);
            }
            catch (DataSourceNotFoundException ex)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.BinderModelName, ex, bindingContext.ModelMetadata);
                return;
            }

            var dataSourceType = dataSource.GetType();

            bindingContext.Result = ModelBindingResult.Success(dataSource);


            // TODO: how  are we determining which properties to inject into a datasource?
            // This is using [CoalesceAttribute] - should this be something else?
            // TODO: pull this logic out of here and into ClassViewModel.
            var desiredPropertyViewModels = 
                new ReflectionTypeViewModel(dataSourceType).ClassViewModel.DataSourceParameters;

            var desiredPropertiesMetadata = desiredPropertyViewModels
                .Select(propViewModel => bindingContext.ModelMetadata.GetMetadataForProperty(dataSourceType, propViewModel.Name))
                .ToList();

            var req = desiredPropertiesMetadata.First();
            var req2 = req.IsBindingRequired;

            bindingContext.ValidationState[dataSource] = new ValidationStateEntry()
            {
                Strategy = new SelectivePropertyComplexObjectValidationStrategy(desiredPropertiesMetadata)
            };

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
            ));

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

                // The alternative to do this would be to make a full copy of ComplexTypeModelBinder and
                // change out the desired pieces.
                await (childBinder
                    .GetType()
                    .GetMethod("BindModelCoreAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(childBinder, new[] { bindingContext }) as Task);
                // await childBinder.BindModelAsync(bindingContext);
            }
        }

        private class SelectivePropertyComplexObjectValidationStrategy : IValidationStrategy
        {
            public SelectivePropertyComplexObjectValidationStrategy(ICollection<ModelMetadata> properties)
            {
                Properties = properties;
            }

            public ICollection<ModelMetadata> Properties { get; }

            /// <inheritdoc />
            public IEnumerator<ValidationEntry> GetChildren(
                ModelMetadata metadata,
                string key,
                object model)
            {
                if (model == null) return Enumerable.Empty<ValidationEntry>().GetEnumerator();

                return Properties
                    .Select(p => new ValidationEntry(p, ModelNames.CreatePropertyModelName(key, p.BinderModelName ?? p.PropertyName), model))
                    .GetEnumerator();
            }
        }
    }
}

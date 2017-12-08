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

namespace IntelliTect.Coalesce.Mvc
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
            var dataSourceDescriminator = bindingContext.BinderModelName;
            if (string.IsNullOrEmpty(dataSourceDescriminator))
            {
                dataSourceDescriminator = "dataSource";
            }

            var valueProviderResult =
                bindingContext.ValueProvider.GetValue(dataSourceDescriminator);

            var requestedDataSource = valueProviderResult.FirstValue;


            var typeViewModel = new ReflectionTypeViewModel(bindingContext.ModelType);
            if (!typeViewModel.IsA(typeof(IDataSource<>))) return;

            var servedType = typeViewModel.GenericArgumentsFor(typeof(IDataSource<>)).Single();

            var dataSource = dataSourceFactory.GetDataSource(servedType.ClassViewModel, requestedDataSource);
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
                await childBinder.BindModelAsync(bindingContext);
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

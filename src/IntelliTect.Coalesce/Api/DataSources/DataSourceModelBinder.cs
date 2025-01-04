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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Collections;
using IntelliTect.Coalesce.Mapping;

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

            List<PropertyViewModel> modelBinderProps = new();
            CrudContext? crudContext = null;
            JsonOptions? jsonOptions = null;
            foreach (var prop in desiredPropertyViewModels)
            {
                var propType = prop.Type;
                string queryParamName = bindingContext.ModelName + "." + prop.Name;

                if (
                    bindingContext.ValueProvider.GetValue(queryParamName) is { Length: 1 } result &&
                    result.Values is [string { Length: > 1 } str] &&
                    (str[0] switch
                    {
                        '{' => propType.IsPOCO,
                        '[' => propType.IsCollection,
                        _ => false
                    })
                )
                {
                    jsonOptions ??= bindingContext.HttpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value ?? new JsonOptions();

                    // TODO: Apply model validation?
                    // TODO: Add tests
                    // TODO: Client support

                    try
                    {
                        Type deserializeTarget = prop.PropertyInfo.PropertyType;
                        if (propType.PureType.ClassViewModel is ClassViewModel pureTypeCvm && !pureTypeCvm.IsCustomDto)
                        {
                            crudContext ??= bindingContext.HttpContext.RequestServices.GetRequiredService<CrudContext>();

                            var entityTypeViewModel = pureTypeCvm.BaseViewModel;
                            var dtoClassViewModel = crudContext.ReflectionRepository.GeneratedParameterDtos[entityTypeViewModel];

                            deserializeTarget = dtoClassViewModel.Type.TypeInfo;
                            var mappingContext = new MappingContext(crudContext);
                            if (propType.IsCollection)
                            {
                                deserializeTarget = typeof(List<>).MakeGenericType(deserializeTarget);

                                object? value = JsonSerializer.Deserialize(str, deserializeTarget, jsonOptions.JsonSerializerOptions);
                                if (value is IList values)
                                {
                                    IList results = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(propType.TypeInfo))!;
                                    foreach (var dto in values)
                                    {
                                        object model = (dto as dynamic).MapToNew(mappingContext);
                                        results.Add(model);
                                    }
                                    if (propType.IsArray)
                                    {
                                        Array array = Array.CreateInstance(propType.TypeInfo, results.Count);
                                        results.GetType().GetMethod("CopyTo", [array.GetType()])!.Invoke(results, [array]);

                                        prop.PropertyInfo.SetValue(dataSource, array);
                                    }
                                    else
                                    {
                                        prop.PropertyInfo.SetValue(dataSource, results);
                                    }
                                }
                            }
                            else
                            {
                                object? dto = JsonSerializer.Deserialize(str, deserializeTarget, jsonOptions.JsonSerializerOptions);
                                object? model = dto == null ? null : ((dynamic)dto).MapToNew(mappingContext);
                                prop.PropertyInfo.SetValue(dataSource, model);
                            }
                        }
                        else
                        {
                            object? value = JsonSerializer.Deserialize(str, deserializeTarget, jsonOptions.JsonSerializerOptions);
                            prop.PropertyInfo.SetValue(dataSource, value);
                        }
                    }
                    // This catch logic from 
                    // https://github.com/dotnet/aspnetcore/blob/c096dbbbe652f03be926502d790eb499682eea13/src/Mvc/Mvc.Core/src/Formatters/SystemTextJsonInputFormatter.cs#L113
                    catch (JsonException jsonException)
                    {
                        var path = jsonException.Path ?? string.Empty;
                        path = queryParamName + '.' + path;

                        Exception modelStateException = jsonOptions.AllowInputFormatterExceptionMessages 
                            ? new InputFormatterException(jsonException.Message, jsonException) 
                            : jsonException;

                        bindingContext.ModelState.TryAddModelError(
                            path, 
                            modelStateException, 
                            bindingContext.ModelMetadata.GetMetadataForProperty(dataSourceType, prop.Name));
                    }
                    catch (Exception exception) when (exception is FormatException || exception is OverflowException)
                    {
                        // The code in System.Text.Json never throws these exceptions. However a custom converter could produce these errors for instance when
                        // parsing a value. These error messages are considered safe to report to users using ModelState.

                        bindingContext.ModelState.TryAddModelError(string.Empty,
                            exception, 
                            bindingContext.ModelMetadata.GetMetadataForProperty(dataSourceType, prop.Name));
                    }

                }
                else
                {
                    modelBinderProps.Add(prop);
                }
            }

            // Get the ASP.NET MVC metadata objects for these properties.
            var desiredPropertiesMetadata = modelBinderProps
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
                // to not occur if the client didn't provide any values for those parameters.

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

using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace IntelliTect.Coalesce.Models
{
    public class ItemResult : ApiResult
    {
        // TODO: incorporate validation issues into the generated typescript
        /// <summary>
        /// A collection of validation issues to send to the client.
        /// Currently, this is not accommodated for in the typescript that is generated.
        /// </summary>
        public ICollection<ValidationIssue>? ValidationIssues { get; set; }

        public ItemResult(): base() { }

        public ItemResult(string? errorMessage) : base(errorMessage) { }

        public ItemResult(ItemResult result) : base(result)
        {
            ValidationIssues = result.ValidationIssues;
        }

        public ItemResult(
            bool wasSuccessful, 
            string? message = null, 
            IEnumerable<ValidationIssue>? validationIssues = null,
            IncludeTree? includeTree = null
        ) 
            : base(wasSuccessful, message)
        {
            ValidationIssues = validationIssues as ICollection<ValidationIssue> ?? validationIssues?.ToList();
            IncludeTree = includeTree;
        }

        /// <summary>
        /// Validate the <see cref="ValidationAttribute"/>s present on <paramref name="model"/>, 
        /// returning an <see cref="ItemResult"/> representing the result of the validation.
        /// </summary>
        /// <param name="model">The model to validate</param>
        /// <param name="deep">Whether to recursively validate nested complex objects.</param>
        /// <param name="forceRequired">
        ///     If true, required properties will be force validated on generated DTOs.
        ///     Otherwise, only properties that were set by the client request are validated on generated DTOs.
        /// </param>
        /// <param name="serviceProvider">An optional service provider that will be passed to custom validation attributes that require one.</param>
        /// <returns>An <see cref="ItemResult"/> representing the result of the validation operation.</returns>
        public static ItemResult FromValidation(
            object model, 
            bool deep = true, 
            bool forceRequired = false, 
            IServiceProvider? serviceProvider = null
        )
        {
            List<ValidationIssue> issues = new();
            ValidateObject(issues, "", model, deep, forceRequired, serviceProvider);

            if (issues.Count > 0)
            {
                return new ItemResult(false, string.Join("\n", issues.Select(i => i.Issue)), issues);
            }
            return true;
        }

        /// <summary>
        /// Validate the <see cref="ValidationAttribute"/>s present on <paramref name="method"/>'s parameters, 
        /// acquiring parameter values from members of <paramref name="model"/>. 
        /// </summary>
        /// <param name="method">The method whose parameters are being validated.</param>
        /// <param name="model">An object containing the parameter values. Typically an anonymous type.</param>
        /// <param name="serviceProvider">An optional service provider that will be passed to custom validation attributes that require one.</param>
        /// <returns>An <see cref="ItemResult"/> representing the result of the validation operation.</returns>
        public static ItemResult FromParameterValidation(
            MethodViewModel method,
            object model,
            IServiceProvider? serviceProvider = null
        ) => FromParameterValidation(method, model, true, true, serviceProvider: serviceProvider);

        /// <summary>
        /// Validate the <see cref="ValidationAttribute"/>s present on <paramref name="method"/>'s parameters, 
        /// acquiring parameter values from members of <paramref name="model"/>. 
        /// </summary>
        /// <param name="method">The method whose parameters are being validated.</param>
        /// <param name="model">An object containing the parameter values. Typically an anonymous type.</param>
        /// <param name="deep">Whether to recursively validate nested complex objects.</param>
        /// <param name="forceRequired">
        ///     If true, required properties will be force validated on generated DTOs. 
        ///     Otherwise, only properties that were set by the client request are validated on generated DTOs.
        /// </param>
        /// <param name="serviceProvider">An optional service provider that will be passed to custom validation attributes that require one.</param>
        /// <returns>An <see cref="ItemResult"/> representing the result of the validation operation.</returns>
        public static ItemResult FromParameterValidation(
            MethodViewModel method,
            object model,
            bool deep,
            bool forceRequired,
            IServiceProvider? serviceProvider = null
        )
        {
            List<ValidationIssue> issues = new();
            ValidateParameters(issues, method, model, deep, forceRequired, serviceProvider);

            if (issues.Count > 0)
            {
                return new ItemResult(false, string.Join("\n", issues.Select(i => i.Issue)), issues);
            }
            return true;
        }

        internal static void ValidateObject(
            List<ValidationIssue> results,
            string prefix,
            object obj,
            bool deep,
            bool forceRequired,
            IServiceProvider? serviceProvider
        )
        {
            var typeViewModel = ReflectionRepository.Global.GetOrAddType(obj.GetType());
            if (typeViewModel.IsCollection)
            {
                // collection of objects
                int i = 0;
                foreach (var childObj in (System.Collections.IEnumerable)obj)
                {
                    if (childObj == null) continue;

                    ValidateObject(
                        results,
                        $"{prefix}[{i}]",
                        childObj,
                        deep,
                        forceRequired,
                        serviceProvider
                    );
                    i++;
                }
                return;
            }

            var cvm = typeViewModel.ClassViewModel;
            if (cvm == null)
            {
#if DEBUG
                throw new ArgumentException("Cannot validate an object of type " + typeViewModel);
#else
                return;
#endif
            }

            // The source of the attributes to use for validation.
            // For generated DTOs (which don't own the attribute),
            // pull the attrs from the model. For custom DTOs,
            // we pull the attributes from the DTO since:
            // 1) The DTO props probably dont map 1-to-1 to the model props
            // 2) The developer wrote the custom DTO and can put attributes directly on it.
            var attributeSource = typeViewModel.IsA(typeof(GeneratedDto<>))
                ? typeViewModel.GenericArgumentsFor(typeof(GeneratedDto<>))![0].ClassViewModel!
                : cvm;

            foreach (var prop in cvm.ClientProperties)
            {
                string propName = prop.Name;
                string fullPropName = (prefix + "." + propName).Trim('.');

                // The property on the underlying model if we're validating a generated dto:
                if (attributeSource.PropertyByName(propName) is not ReflectionPropertyViewModel sourceProp) continue;

                if (
                    // Skip properties that the client is not permitted to send:
                    !sourceProp.IsClientSerializable
                )
                {
                    continue;
                }

                if (obj is ISparseDto genDto && !forceRequired && !genDto.ChangedProperties.Contains(propName))
                {
                    // 99% case: For generated DTOs, normally only validate the properties that the user is changing.
                    // The exception to this case is when we want to ensure we're validating all required props,
                    // which happens for custom method calls and create-saves, in which case `forceRequired==true`.
                    continue;
                }

                var value = prop.PropertyInfo.GetValue(obj);
                var validationContext = new ValidationContext(obj, serviceProvider, null)
                {
                    MemberName = propName,
                    DisplayName = sourceProp.DisplayName
                };

                foreach (var attr in sourceProp.GetValidationAttributes())
                {
                    var result = attr.GetValidationResult(value, validationContext);
                    if (result != null && result != ValidationResult.Success)
                    {
                        string message = result.ErrorMessage ?? "An unknown validation error occurred.";
                        results.Add(new ValidationIssue(fullPropName, message));
                    }
                }

                if (deep && value != null && prop.Object != null)
                {
                    ValidateObject(
                        results,
                        fullPropName,
                        value,
                        deep,
                        forceRequired,
                        serviceProvider
                    );
                }
            }
        }

        internal static void ValidateParameters(
            List<ValidationIssue> results,
            MethodViewModel method,
            object obj,
            bool deep,
            bool forceRequired,
            IServiceProvider? serviceProvider
        )
        {
            foreach (var param in method.ClientParameters.OfType<ReflectionParameterViewModel>())
            {
                string propName = param.Name;
                var value = obj.GetType().GetProperty(param.CsParameterName)?.GetValue(obj);

                var validationContext = new ValidationContext(obj, serviceProvider, null)
                {
                    MemberName = propName,
                    DisplayName = param.DisplayName
                };
                foreach (var attr in param.GetValidationAttributes())
                {
                    var result = attr.GetValidationResult(value, validationContext);
                    if (result != null && result != ValidationResult.Success)
                    {
                        string message = result.ErrorMessage ?? "An unknown validation error occurred.";
                        results.Add(new ValidationIssue(propName, message));
                    }
                }

                if (deep && value != null && param.PureType.HasClassViewModel)
                {
                    ValidateObject(
                        results,
                        propName,
                        value,
                        deep,
                        forceRequired,
                        serviceProvider
                    );
                }
            }
        }

        public static implicit operator ItemResult(bool success) => new ItemResult(success);

        public static implicit operator ItemResult(string errorMessage) => new ItemResult(errorMessage);
    }

    public class ItemResult<T> : ItemResult
    {
#if NETCOREAPP
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [System.Diagnostics.CodeAnalysis.MaybeNull]
#endif 
        public T Object { get; set; }

        /// <summary>
        /// For bulk save responses, holds a mapping between the incoming refs of 
        /// unkeyed bulk save items and each item's new primary key.
        /// </summary>
        public IDictionary<int, object?>? RefMap { get; set; }

        public ItemResult(): base() { }

        public ItemResult(string? errorMessage) : base(errorMessage) { }

        public ItemResult(
            ItemResult result,
#if NETCOREAPP
            [System.Diagnostics.CodeAnalysis.AllowNull]
#endif  
            T obj = default
        ) : base(result)
        {
            Object = obj;
        }

        public ItemResult(
            bool wasSuccessful, 
            string? message = null,
#if NETCOREAPP
            [System.Diagnostics.CodeAnalysis.AllowNull]
#endif 
            T obj = default, 
            IEnumerable<ValidationIssue>? validationIssues = null,
            IncludeTree? includeTree = null
        ) 
            : base(wasSuccessful, message, validationIssues, includeTree)
        {
            Object = obj;
        }

        public ItemResult(T obj, IncludeTree? includeTree = null) : this(true, includeTree: includeTree)
        {
            Object = obj;
        }

        public static implicit operator ItemResult<T>(bool success) => new ItemResult<T>(success);

        public static implicit operator ItemResult<T>(string? errorMessage) => new ItemResult<T>(errorMessage);

        public static implicit operator ItemResult<T>(T obj) => new ItemResult<T>(obj);
    }
}
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


// Intentionally in this namespace for ease of discovery and use.
// While these methods are not used by the core of Coalesce, 
// they are intended to reduce the burden on anyone who needs to access DTO properties in custom IBehaviors<T>.

namespace IntelliTect.Coalesce
{
    public static class DtoExtensions
    {
        /// <summary>
        /// Get the value of a value type on a generic DTO. Allows accessing properties where the implementation type of the DTO is not known.
        /// </summary>
        public static TValue? GetValue<TObject, TValue>(this IParameterDto<TObject> dto, Expression<Func<TObject, TValue>> property)
            where TObject : class
            where TValue : struct
            => GetPropertyInfo(dto, property).GetValue(dto) as TValue?;

        /// <summary>
        /// Get the value of a value type on a generic DTO. 
        /// Allows accessing properties where the implementation type of the DTO is not known.
        /// </summary>
        public static TValue? GetValue<TObject, TValue>(this IParameterDto<TObject> dto, Expression<Func<TObject, TValue?>> property)
            where TObject : class
            where TValue : struct
            => GetPropertyInfo(dto, property).GetValue(dto) as TValue?;

        /// <summary>
        /// Get the value of a string property on a generic DTO. 
        /// Allows accessing properties where the implementation type of the DTO is not known.
        /// </summary>
        public static string GetValue<TObject>(this IParameterDto<TObject> dto, Expression<Func<TObject, string>> property)
            where TObject : class
            // This is just a "nice" alias for GetObject so you can still invoke a method called "GetValue" for strings.
            => dto.GetObject(property);

        /// <summary>
        /// Get the value of an object property on a generic DTO. 
        /// Allows accessing properties where the implementation type of the DTO is not known.
        /// </summary>
        public static TValue GetObject<TObject, TValue>(this IParameterDto<TObject> dto, Expression<Func<TObject, TValue>> property)
            where TObject : class
            where TValue : class
            => (TValue)GetPropertyInfo(dto, property).GetValue(dto)!;

        /// <summary>
        /// Check if the value of a property on a DTO is not null.
        /// Allows accessing properties where the implementation type of the DTO is not known.
        /// </summary>
        public static bool HasValue<TObject, TValue>(this IParameterDto<TObject> dto, Expression<Func<TObject, TValue>> property)
            where TObject : class
            => !(GetPropertyInfo(dto, property).GetValue(dto) is null);


        private static PropertyInfo GetPropertyInfo(object dto, LambdaExpression property)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            Type dtoType = dto.GetType();
            string propertyName = property.GetExpressedProperty(paramType: null).Name;
            var propViewModel = ReflectionRepository.Global
                .GetClassViewModel(dtoType)?
                .PropertyByName(propertyName);

            if (propViewModel == null) 
                throw new ArgumentException($"Property {propertyName} doesn't exist on DTO {dto.GetType().FullName}");

            return propViewModel.PropertyInfo;
        }
    }
}

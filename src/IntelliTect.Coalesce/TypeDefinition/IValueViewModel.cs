using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public interface IValueViewModel : IAttributeProvider
    {
        string Name { get; }

        string JsVariable { get; }

        string DisplayName { get; }
        
        string? Description { get; }

        /// <summary>
        /// Gets the raw, unaltered type of the value.
        /// </summary>
        TypeViewModel Type { get; }

        /// <summary>
        /// Gets the type without any collection around it.
        /// </summary>
        TypeViewModel PureType { get; }

        bool IsRequired { get; }
    }

    public static class ValueViewModelExtensions
    {
        /// <summary>
        /// Returns the MinLength of the property or null if it doesn't exist.
        /// </summary>
        public static int? GetValidationMinLength(this IValueViewModel v) => v.GetAttributeValue<MinLengthAttribute, int>(a => a.Length);

        /// <summary>
        /// Returns the MaxLength of the property or null if it doesn't exist.
        /// </summary>
        public static int? GetValidationMaxLength(this IValueViewModel v) => v.GetAttributeValue<MaxLengthAttribute, int>(a => a.Length);

        /// <summary>
        /// Returns the range of valid values or null if they don't exist. (min, max)
        /// </summary>
        public static Tuple<object, object>? GetValidationRange(this IValueViewModel v)
        {
            var min = v.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Minimum));
            var max = v.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Maximum));
            if (min != null && max != null) return new Tuple<object, object>(min, max);
            return null;
        }

        /// <summary>
        /// Common IsRequired logic for all IValueViewModel.
        /// </summary>
        internal static bool IsRequired(this IValueViewModel v)
        {
            if (v.HasAttribute<RequiredAttribute>()) return true;

            if (
                v.Type.IsNumber && 
                !v.Type.IsNullableValueType && 
                v.GetValidationRange() is var range and not null && (
                    Convert.ToDecimal(range.Item1) > 0 ||
                    Convert.ToDecimal(range.Item2) < 0
                )
            )
            {
                // Value is a non-nullable number with a [RangeAttribute] that excludes zero.
                // Coalesce does not implicitly require non-nullable value types - it allows them to stay as their default value.

                // Normally, our validation would allow this to fall onto its default value if
                // not provided by the client to a generated DTO (since RangeAttribute ignores null
                // and the property on the DTO is null if the value is missing). But, that's clearly 
                // not user intent if they excluded zero from the valid range.

                // So, add an implicit required in this case.
                return true;
            }

            return false;
        }
    }
}

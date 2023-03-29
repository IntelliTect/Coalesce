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
    }
}

using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class ValueViewModel : IAttributeProvider
    {
        protected ValueViewModel(TypeViewModel type)
        {
            Type = type;
        }

        public abstract string Name { get; }

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public virtual string DisplayName =>
            this.GetAttributeValue<DisplayNameAttribute>(a => a.DisplayName) ??
            this.GetAttributeValue<DisplayAttribute>(a => a.Name) ??
            Name.ToProperCase();

        /// <summary>
        /// Returns the description from the DisplayAttribute or DescriptionAttribute, if present.
        /// </summary>
        public virtual string? Description => 
            this.GetAttributeValue<DisplayAttribute>(a => a.Description) ?? 
            this.GetAttributeValue<DescriptionAttribute>(a => a.Description);

        public virtual string JsVariable => Name.ToCamelCase();

        /// <summary>
        /// Gets the raw, unaltered type of the value.
        /// </summary>
        public TypeViewModel Type { get; init; }

        /// <summary>
        /// Gets the type without any collection around it.
        /// </summary>
        public TypeViewModel PureType => Type.PureType;

        /// <summary>
        /// True if the value should generate a "required" validation rule.
        /// </summary>
        public virtual bool IsRequired
        {
            get
            {

                if (this.HasAttribute<RequiredAttribute>()) return true;

                if (
                    Type.IsNumber &&
                    !Type.IsNullableValueType &&
                    this.GetAttribute<RangeAttribute>() is var range and not null &&
                    Convert.ToDouble(range.GetValue(r => r.Minimum) ?? 0) is double min &&
                    Convert.ToDouble(range.GetValue(r => r.Maximum) ?? 0) is double max &&
                    (
                        min > 0 ||
                        max < 0
#if NET8_0_OR_GREATER
                        || range.GetValue(a => a.MinimumIsExclusive) == true && min == 0
                        || range.GetValue(a => a.MaximumIsExclusive) == true && max == 0
#endif
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

        /// <summary>
        /// Returns the range of valid values or null if they don't exist. (min, max)
        /// </summary>
        public Tuple<object, object>? Range
        {
            get
            {
                var min = this.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Minimum));
                var max = this.GetAttributeValue<RangeAttribute>(nameof(RangeAttribute.Maximum));
                if (min != null && max != null) return new Tuple<object, object>(min, max);
                return null;
            }
        }

        public DateTypeAttribute.DateTypes? DateType =>
            this.GetAttributeValue<DateTypeAttribute, DateTypeAttribute.DateTypes>(a => a.DateType) ??
            (this.GetAttributeValue<DataTypeAttribute, DataType>(a => a.DataType) switch
            {
                DataType.Date => DateTypeAttribute.DateTypes.DateOnly,
                DataType.Time => DateTypeAttribute.DateTypes.TimeOnly,
                _ => (DateTypeAttribute.DateTypes?)null
            }) ?? Type.DateType;

        /// <summary>
        /// Returns the MinLength of the property or null if it doesn't exist.
        /// </summary>
        public int? MinLength => this.GetAttributeValue<MinLengthAttribute, int>(a => a.Length);

        /// <summary>
        /// Returns the MaxLength of the property or null if it doesn't exist.
        /// </summary>
        public int? MaxLength => this.GetAttributeValue<MaxLengthAttribute, int>(a => a.Length);

        public abstract IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>() where TAttribute : Attribute;
    }
}

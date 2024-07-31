using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Validation attributes to be applied on the client via KnockoutValidation.
    /// </summary>    
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ClientValidationAttribute: System.Attribute
    {
        // Referenced at: https://github.com/Knockout-Contrib/Knockout-Validation/wiki/Native-Rules
        // Once an item is added here, update the script creation logic in the PropertyViewModel class.
        // Properties need to be primitive types, thus no nullables.
        /// <summary>
        /// If true, even when these validation fail, it will still save the value. Default is false;
        /// If true the client handles these as warnings.
        /// </summary>
        [Obsolete("AllowSave only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public bool AllowSave { get; set; } = false;
        public bool IsRequired { get; set; }

        // Note: nullable propeties can't be used here because nullable values can't be used as attribute initializers.
        public double MinValue { get; set; } = double.MaxValue;
        public double MaxValue { get; set; } = double.MinValue;
        public int MinLength { get; set; } = int.MaxValue;
        public int MaxLength { get; set; } = int.MinValue;

        public string? Pattern { get; set; }
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public double Step { get; set; }
        public bool IsEmail { get; set; }
        public bool IsPhoneUs { get; set; }

        /// <summary>
        /// This is an unquoted string when converted to JS. Use '' to specify strings.
        /// </summary>
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public string? Equal { get; set; }

        /// <summary>
        /// This is an unquoted string when converted to JS. Use '' to specify strings.
        /// </summary>
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public string? NotEqual { get; set; }
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public bool IsDate { get; set; }
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public bool IsDateIso { get; set; }
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public bool IsNumber { get; set; }
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public bool IsDigit { get; set; }

        /// <summary>
        /// Allows specifying a name used in a custom validation. .extend({ CustomName: CustomValue })
        /// </summary>
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public string? CustomName { get; set; }

        /// <summary>
        /// Allows specifying a value used in a custom validation. .extend({ CustomName: CustomValue })
        /// </summary>
        [Obsolete("This property only affects the Knockout stack for Coalesce. It does not affect Vue applications.")]
        public string? CustomValue { get; set; }

        /// <summary>
        /// Gets or sets an error message to associate with a validation control if validation fails.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

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
        public bool AllowSave { get; set; } = false;
        public bool IsRequired { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? Pattern { get; set; }
        public double Step { get; set; }
        public bool IsEmail { get; set; }
        public bool IsPhoneUs { get; set; }

        /// <summary>
        /// This is an unquoted string when converted to JS. Use '' to specify strings.
        /// </summary>
        public string? Equal { get; set; }

        /// <summary>
        /// This is an unquoted string when converted to JS. Use '' to specify strings.
        /// </summary>
        public string? NotEqual { get; set; }
        public bool IsDate { get; set; }
        public bool IsDateIso { get; set; }
        public bool IsNumber { get; set; }
        public bool IsDigit { get; set; }

        /// <summary>
        /// Allows specifying a name used in a custom validation. .extend({ CustomName: CustomValue })
        /// </summary>
        public string? CustomName { get; set; }

        /// <summary>
        /// Allows specifying a value used in a custom validation. .extend({ CustomName: CustomValue })
        /// </summary>
        public string? CustomValue { get; set; }

        /// <summary>
        /// Gets or sets an error message to associate with a validation control if validation fails.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

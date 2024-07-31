using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ClientValidationAttribute: System.Attribute
    {
        public bool IsRequired { get; set; }

        // Note: nullable propeties can't be used here because nullable values can't be used as attribute initializers.
        public double MinValue { get; set; } = double.MaxValue;
        public double MaxValue { get; set; } = double.MinValue;
        public int MinLength { get; set; } = int.MaxValue;
        public int MaxLength { get; set; } = int.MinValue;

        public string? Pattern { get; set; }
        public bool IsEmail { get; set; }
        public bool IsPhoneUs { get; set; }

        /// <summary>
        /// Gets or sets an error message to associate with a validation control if validation fails.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

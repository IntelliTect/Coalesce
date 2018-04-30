using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Validation
{
    internal class ValidationResult
    {
        public bool WasSuccessful { get; set; }
        public bool IsWarning { get; set; }
        public string Area { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            if (WasSuccessful)
            {
                return $"  Success: {Area}: {Message}";
            }
            if (IsWarning)
            {
                return $"-- Warning: {Area}: {Message}";
            }
            return $"** Failure: {Area}: {Message}";
        }
    }
}

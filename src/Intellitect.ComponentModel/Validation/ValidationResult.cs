using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Validation
{
    public class ValidationResult
    {
        public bool WasSuccessful { get; set; }
        public bool isWarning { get; set; }
        public string Area { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            if (WasSuccessful)
            {
                return $"  Success: {Area}: {Message}";
            }
            if (isWarning)
            {
                return $"--Warning: {Area}: {Message}";
            }
            return $"**Failure: {Area}: {Message}";
        }
    }
}

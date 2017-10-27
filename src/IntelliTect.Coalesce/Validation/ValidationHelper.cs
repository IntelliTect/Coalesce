using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Validation
{
    /// <summary>
    /// Evaluates various conditions and keeps a list of what succeeded and failed.
    /// </summary>
    public class ValidationHelper: List<ValidationResult>
    {
        public string Area { get; set; } = "";

        public bool IsTrue(bool expression, string message, bool isWarning = false)
        {
            bool result;
            if (expression)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            this.Add(new ValidationResult() { WasSuccessful = result, IsWarning = isWarning, Area = Area, Message = message });

            return result;
        }

        public bool AreNotEqual(object obj1, object obj2, string message, bool isWarning = false)
        {
            return IsTrue(obj1 != obj2, message, isWarning);
        }
        public bool IsNotNull(object obj, string message, bool isWarning = false)
        {
            return IsTrue(obj != null, message, isWarning);
        }
        public bool IsNull(object obj, string message, bool isWarning = false)
        {
            return IsTrue(obj == null, message, isWarning);
        }

        public bool isFalse(bool expression, string message, bool isWarning = false)
        {
            return IsTrue(!expression, message, isWarning);
        }
    }
}

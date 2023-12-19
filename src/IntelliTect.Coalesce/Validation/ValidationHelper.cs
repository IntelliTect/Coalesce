using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.Validation
{
    /// <summary>
    /// Evaluates various conditions and keeps a list of what succeeded and failed.
    /// </summary>
    internal class ValidationHelper: List<ValidationResult>
    {
        public string Area { get; set; } = "";

        public bool IsTrue(bool? expression, string message, bool isWarning = false)
        {
            bool result;
            if (expression == true)
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

        public bool AreNotEqual(object? obj1, object? obj2, string message, bool isWarning = false)
        {
            return IsTrue(obj1 != obj2, message, isWarning);
        }

        public bool IsNotNull(
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] 
            object? obj, 
            string message, 
            bool isWarning = false)
        {
            return IsTrue(obj != null, message, isWarning);
        }

        public bool IsNull(
            [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] 
            object? obj, 
            string message, 
            bool isWarning = false)
        {
            return IsTrue(obj == null, message, isWarning);
        }

        public bool IsFalse(bool expression, string message, bool isWarning = false)
        {
            return IsTrue(!expression, message, isWarning);
        }

        public void NoDuplicates<T, TProp>(IEnumerable<T> items, Expression<Func<T, TProp>> selector, IEqualityComparer<TProp>? comparer = null)
        {
            var grouped = comparer is null 
                ? items.ToLookup(selector.Compile())
                : items.ToLookup(selector.Compile(), comparer);

            foreach (var grouping in grouped)
            {
                IsTrue(
                    grouping.Count() == 1,
                    $"{selector.GetExpressedProperty().Name} of {typeof(T).Name.Replace("ViewModel", "")} must be distinct. " +
                    $"Conflicting values: {string.Concat(grouping.Select(i => "\n - " + i))}");
            }
        }
    }
}

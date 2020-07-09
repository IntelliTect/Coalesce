using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Models
{
    public class ItemResult : ApiResult
    {
        // TODO: incorporate validation issues into the generated typescript
        /// <summary>
        /// A collection of validation issues to send to the client.
        /// Currently, this is not accommodated for in the typescript that is generated.
        /// </summary>
        public ICollection<ValidationIssue>? ValidationIssues { get; set; }

        public ItemResult(): base() { }

        public ItemResult(string? message) : base(message) { }

        public ItemResult(ItemResult result) : base(result)
        {
            ValidationIssues = result.ValidationIssues;
        }

        public ItemResult(bool wasSuccessful, string? message = null, IEnumerable<ValidationIssue>? validationIssues = null) 
            : base(wasSuccessful, message)
        {
            ValidationIssues = validationIssues as ICollection<ValidationIssue> ?? validationIssues?.ToList();
        }

        public static implicit operator ItemResult(bool success) => new ItemResult(success);

        public static implicit operator ItemResult(string message) => new ItemResult(message);
    }

    public class ItemResult<T> : ItemResult
    {
#if NETCOREAPP3_1
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [System.Diagnostics.CodeAnalysis.MaybeNull]
#endif 
        public T Object { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ItemResult(): base() { }

        public ItemResult(string? message) : base(message) { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ItemResult(
            ItemResult result,
#if NETCOREAPP3_1
            [System.Diagnostics.CodeAnalysis.AllowNull]
#endif  
            T obj = default
        ) : base(result)
        {
            Object = obj;
        }

        public ItemResult(
            bool wasSuccessful, 
            string? message = null,
#if NETCOREAPP3_1
            [System.Diagnostics.CodeAnalysis.AllowNull]
#endif 
            T obj = default, 
            IEnumerable<ValidationIssue>? validationIssues = null
        ) 
            : base(wasSuccessful, message, validationIssues)
        {
            Object = obj;
        }

        public ItemResult(T obj) : this(true)
        {
            Object = obj;
        }

        public static implicit operator ItemResult<T>(bool success) => new ItemResult<T>(success);

        public static implicit operator ItemResult<T>(string? message) => new ItemResult<T>(message);
    }
}
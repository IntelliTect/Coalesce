﻿using System;
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

        public ItemResult(string? errorMessage) : base(errorMessage) { }

        public ItemResult(ItemResult result) : base(result)
        {
            ValidationIssues = result.ValidationIssues;
            IncludeTree = result.IncludeTree;
        }

        public ItemResult(
            bool wasSuccessful, 
            string? message = null, 
            IEnumerable<ValidationIssue>? validationIssues = null,
            IncludeTree? includeTree = null
        ) 
            : base(wasSuccessful, message)
        {
            ValidationIssues = validationIssues as ICollection<ValidationIssue> ?? validationIssues?.ToList();
            IncludeTree = includeTree;
        }

        public static implicit operator ItemResult(bool success) => new ItemResult(success);

        public static implicit operator ItemResult(string errorMessage) => new ItemResult(errorMessage);
    }

    public class ItemResult<T> : ItemResult
    {
#if NETCOREAPP
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [System.Diagnostics.CodeAnalysis.MaybeNull]
#endif 
        public T Object { get; set; }

        public ItemResult(): base() { }

        public ItemResult(string? errorMessage) : base(errorMessage) { }

        public ItemResult(
            ItemResult result,
#if NETCOREAPP
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
#if NETCOREAPP
            [System.Diagnostics.CodeAnalysis.AllowNull]
#endif 
            T obj = default, 
            IEnumerable<ValidationIssue>? validationIssues = null,
            IncludeTree? includeTree = null
        ) 
            : base(wasSuccessful, message, validationIssues, includeTree)
        {
            Object = obj;
        }

        public ItemResult(T obj, IncludeTree? includeTree = null) : this(true, includeTree: includeTree)
        {
            Object = obj;
        }

        public static implicit operator ItemResult<T>(bool success) => new ItemResult<T>(success);

        public static implicit operator ItemResult<T>(string? errorMessage) => new ItemResult<T>(errorMessage);

        public static implicit operator ItemResult<T>(T obj) => new ItemResult<T>(obj);
    }
}
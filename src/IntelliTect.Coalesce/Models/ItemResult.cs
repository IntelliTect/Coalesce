using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Models
{
    public class ItemResult : ApiResult
    {
        
        public ItemResult(): base() { }

        public ItemResult(bool wasSuccessful) : base(wasSuccessful) { }

        public ItemResult(string problem) : base(problem) { }

        public ItemResult(Exception ex) : this(false)
        {
            Message = ex.Message;
        }
        
        // TODO: incorporate validation issues into the generated typescript

        /// <summary>
        /// A collection of validation issues to send to the client.
        /// Currently, this is not accommodated for in the typescript that is generated.
        /// </summary>
        public ICollection<ValidationIssue> ValidationIssues { get; set; }

        public static implicit operator ItemResult(bool success)
        {
            return new ItemResult(success);
        }

        public static implicit operator ItemResult(string message)
        {
            return new ItemResult(message);
        }
    }

    public class ItemResult<T> : ItemResult
    {
        public T Object { get; set; }

        public ItemResult(): base() { }

        public ItemResult(bool wasSuccessful) : base(wasSuccessful) { }

        public ItemResult(string problem) : base(problem) { }

        public ItemResult(bool wasSuccessful, T obj) : base(wasSuccessful)
        {
            Object = obj;
        }

        public ItemResult(Exception ex) : base(ex)
        {
        }

        public ItemResult(ItemResult result)
        {
            WasSuccessful = result.WasSuccessful;
            Message = result.Message;
        }

        public static implicit operator ItemResult<T>(bool success)
        {
            return new ItemResult<T>(success);
        }

        public static implicit operator ItemResult<T>(string message)
        {
            return new ItemResult<T>(message);
        }
    }
}
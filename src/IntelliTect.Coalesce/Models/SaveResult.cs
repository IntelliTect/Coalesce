using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Models
{
    public class ItemResult : ApiResult
    {
        // TODO (FUTURE): When behaviors are implemented,
        // bring validation issues back into this object, and incorporate them into the generated typescript.

        //public List<ValidationIssue> ValidationIssues { get; set; }
        
        public ItemResult(): base() { }

        public ItemResult(bool wasSuccessful) : base(wasSuccessful) { }

        public ItemResult(string problem) : base(problem) { }

        public ItemResult(Exception ex) : this(false)
        {
            Message = ex.Message;
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
    }
}
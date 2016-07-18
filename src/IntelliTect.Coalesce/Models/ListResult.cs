using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Models
{
    public class ListResult
    {
        // This is not generic because we can hand back partial objects based on the fields property.
        public bool WasSuccessful { get; set; }
        public string Message { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }

        public IEnumerable List { get; set; }

        public ListResult(): base()
        {
        }

        public ListResult(IEnumerable objs) 
        {
            List = objs;
            WasSuccessful = true;
        }

        public ListResult(IEnumerable objs, int page, int totalCount, int pageSize)
        {
            List = objs;
            WasSuccessful = true;
            Page = page;
            TotalCount = totalCount;
            PageSize = pageSize;
            PageCount = (int)Math.Ceiling((double)totalCount/ pageSize);
        }

        public ListResult(Exception ex) 
        {
            Message = ex.Message;
            WasSuccessful = false;
        }
    }
}
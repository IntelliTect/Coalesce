using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Models
{
    public class ListResult<T> : ApiResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }

        public IList<T> List { get; set; }

        public ListResult() : base()
        {
        }

        public ListResult(IList<T> items) 
        {
            List = items;
            WasSuccessful = true;
        }

        public ListResult(IList<T> items, int page, int totalCount, int pageSize)
        {
            List = items;
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
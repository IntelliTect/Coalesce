using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Models
{
    public interface IListResult : IApiResult
    {
        int Page { get; }
        int PageSize { get; }
        int PageCount { get; }
        int TotalCount { get; }
    }

    public class ListResult<T> : ApiResult, IListResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageCount => (TotalCount + PageSize - 1) / PageSize;
        public int TotalCount { get; set; }

        public IList<T> List { get; set; }
        
        public ListResult(): base() { }

        public ListResult(bool wasSuccessful, string message = null) : base(wasSuccessful, message) { }

        public ListResult(string message) : base(message) { }

        public ListResult(IListResult result, IList<T> items = null)
            : this(items, page: result.Page, pageSize: result.PageSize, totalCount: result.TotalCount, wasSuccessful: result.WasSuccessful, message: result.Message) { }

        public ListResult(IList<T> items, int page, int totalCount, int pageSize, bool wasSuccessful = true, string message = null)
        {
            List = items;
            WasSuccessful = wasSuccessful;
            Message = message;
            Page = page;
            TotalCount = totalCount;
            PageSize = pageSize;
        }

        /// <summary>
        /// Constructs a ListResult from a query that does not have any paging applied for the given page and pageSize.
        /// </summary>
        /// <param name="query">A query without skips or takes applied.</param>
        /// <param name="page">The page number to page to.</param>
        /// <param name="pageSize">The number of items per page.</param>
        public ListResult(IQueryable<T> query, int page, int pageSize)
        {
            TotalCount = query.Count();

            pageSize = pageSize < 1 ? 1 : pageSize;
            // Cap the page number at the last item
            if ((page - 1) * pageSize > TotalCount)
            {
                page = (int)((TotalCount - 1) / pageSize) + 1;
            }

            if (page > 1)
            {
                // Only skip if we're after the first page.
                query = query.Skip((page - 1) * pageSize);
            }

            List = query.Take(pageSize).ToList();

            Page = page;
            PageSize = pageSize;
        }
    }
}
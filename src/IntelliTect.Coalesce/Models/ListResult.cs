using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Models;

public abstract class ListResult : ApiResult
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int PageCount =>
        TotalCount == -1 ? -1
        : PageSize == 0 ? 0
        : (TotalCount + PageSize - 1) / PageSize;

    public int TotalCount { get; set; }

    public ListResult() { }

    public ListResult(bool wasSuccessful, string? message = null) : base(wasSuccessful, message) { }

    public ListResult(string errorMessage) : base(errorMessage) { }

    public ListResult(ApiResult result) : base(result) { }

    public ListResult(ListResult result) : base(result)
    {
        Page = result.Page;
        TotalCount = result.TotalCount;
        PageSize = result.PageSize;
    }
}

public class ListResult<T> : ListResult
{
    public IList<T>? List { get; set; }
    
    public ListResult() { }

    public ListResult(bool wasSuccessful, string? message = null) : base(wasSuccessful, message) { }

    public ListResult(string errorMessage) : base(errorMessage) { }

    public ListResult(ApiResult result) : base(result) { }

    public ListResult(ListResult result, IList<T>? items = null) : base(result)
    {
        List = items;
    }

    public ListResult(IList<T>? items, int page, int totalCount, int pageSize, bool wasSuccessful = true, string? message = null)
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
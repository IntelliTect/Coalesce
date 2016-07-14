using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntelliTect.Coalesce.Interfaces;

namespace IntelliTect.Coalesce.Models
{
    public class GenericListResult<T, TDto> : ListResult
        where TDto : IClassDto<T, TDto>
    {
        public new IEnumerable<TDto> List { get; set; }

        public GenericListResult() : base() { }

        public GenericListResult(IEnumerable<TDto> objs) : base(objs) { }

        public GenericListResult(IEnumerable<TDto> objs, int page, int totalCount, int pageSize)
            : base (objs, page, totalCount, pageSize) { }

        public GenericListResult(Exception ex): base(ex) { }

        public GenericListResult(ListResult result)
        {
            WasSuccessful = true;
            List = (IEnumerable<TDto>)result.List;
            Page = result.Page;
            TotalCount = result.TotalCount;
            PageSize = result.PageSize;
            PageCount = result.PageCount;
        }
    }
}
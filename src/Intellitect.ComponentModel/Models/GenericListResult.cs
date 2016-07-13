using Intellitect.ComponentModel.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Intellitect.ComponentModel.Models
{
    public class GenericListResult<T> : ListResult
        where T : IClassDto
    {
        public new IEnumerable<T> List { get; set; }

        public GenericListResult() : base() { }

        public GenericListResult(IEnumerable<T> objs) : base(objs) { }

        public GenericListResult(IEnumerable<T> objs, int page, int totalCount, int pageSize)
            : base (objs, page, totalCount, pageSize) { }

        public GenericListResult(Exception ex): base(ex) { }

        public GenericListResult(ListResult result)
        {
            WasSuccessful = true;
            List = (IEnumerable<T>)result.List;
            Page = result.Page;
            TotalCount = result.TotalCount;
            PageSize = result.PageSize;
            PageCount = result.PageCount;
        }
    }
}
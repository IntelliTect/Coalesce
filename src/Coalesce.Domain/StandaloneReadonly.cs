using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    [Coalesce, StandaloneEntity]
    public class StandaloneReadonly
    {
        public int Id { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains), ListText]
        public string Name { get; set; } = "";

        [DefaultOrderBy]
        public string Description { get; set; } = "";


        public class DefaultSource : StandardDataSource<StandaloneReadonly>
        {
            public DefaultSource(CrudContext context) : base(context)
            {
            }

            public override async Task<IQueryable<StandaloneReadonly>> GetQueryAsync(IDataSourceParameters parameters)
            {
                return Enumerable.Range(1, 100)
                    .Select(i => new StandaloneReadonly
                    {
                        Id = i,
                        Name = $"Item {i}",
                        Description = $"The {i}th item"
                    })
                    .AsQueryable();
            }
        }
    }
}

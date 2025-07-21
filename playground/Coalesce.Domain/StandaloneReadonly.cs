﻿using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain;

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

        public override Task<IQueryable<StandaloneReadonly>> GetQueryAsync(IDataSourceParameters parameters)
        {
            return Task.FromResult(Enumerable.Range(1, 100)
                .Select(i => new StandaloneReadonly
                {
                    Id = i,
                    Name = $"Item {i}",
                    Description = $"The {i}th item"
                })
                .AsQueryable());
        }
    }
}

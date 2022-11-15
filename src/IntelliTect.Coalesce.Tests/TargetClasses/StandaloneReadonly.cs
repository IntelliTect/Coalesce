using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    [Coalesce, StandaloneEntity]
    public class StandaloneReadonly
    {
        public int Id { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains), ListText]
        public string Name { get; set; } = "";

        [DefaultOrderBy]
        public string Description { get; set; } = "";

        [Coalesce]
        public Task<ItemResult<int>> InstanceMethod(
            [Inject] WeatherService weather,
            [Inject] AppDbContext explicitDbInjection,
            AppDbContext implicitDbInjection
        )
        {
            return Task.FromResult(new ItemResult<int>(42));
        }

        [Coalesce]
        public static Task<ItemResult<int>> StaticMethod(
            [Inject] WeatherService weather,
            [Inject] AppDbContext explicitDbInjection,
            AppDbContext implicitDbInjection
        )
        {
            return Task.FromResult(new ItemResult<int>(42));
        }

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
}

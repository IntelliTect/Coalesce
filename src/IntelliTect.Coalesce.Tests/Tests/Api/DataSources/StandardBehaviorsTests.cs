using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Api.DataSources
{
    public class StandardBehaviorsTests : TestDbContextFixture
    {
        public StandardDataSource<Case, AppDbContext> CaseSource { get; }

        public StandardBehaviorsTests() : base()
        {
            CaseSource = Source<Case>();
        }

        private StandardDataSource<T, AppDbContext> Source<T>()
            where T : class, new()
            => new StandardDataSource<T, AppDbContext>(CrudContext);

        private StandardBehaviors<T, AppDbContext> Behaviors<T>()
            where T : class, new()
            => new StandardBehaviors<T, AppDbContext>(CrudContext);

        private class TransformDs : StandardDataSource<Case, AppDbContext>
        {
            public TransformDs(CrudContext<AppDbContext> context) : base(context) { }

            public override Task TransformResultsAsync(IReadOnlyList<Case> results, IDataSourceParameters parameters)
            {
                foreach (var result in results)
                {
                    // Normally you would never transform a DB column here,
                    // but we need something obvious and easily assertable
                    result.Title = "TRANSFORMED";
                }
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Save_HydratesResultWithDataSourceTransformAsync()
        {
            var dto = new TestDto<Case>(1, c => { c.Description = "new desc"; });
            var ds = new TransformDs(CrudContext)
                .AddModel(m => m.Title, "original title");
            var result = await Behaviors<Case>().SaveAsync(dto, ds, new DataSourceParameters());

            result.AssertSuccess();
            Assert.Equal("new desc", result.Object.SourceEntity.Description);
            Assert.Equal("TRANSFORMED", result.Object.SourceEntity.Title);
        }
    }
}

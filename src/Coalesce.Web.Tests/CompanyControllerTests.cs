using Coalesce.Domain;
using Coalesce.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Web.Tests
{
    [Collection("Database collection")]
    public class CompanyControllerTests : IClassFixture<DatabaseFixtureLocalDb>
    {
        public CompanyControllerTests(DatabaseFixtureLocalDb dbFixture)
        {
            DbFixture = dbFixture;
        }
        private DatabaseFixtureLocalDb DbFixture { get; set; }

        [Fact]
        public async void ListTest()
        {
            CompanyController c = new CompanyController();
            Assert.NotNull(c);
            c.Db = DbFixture.Db;

            Assert.NotNull(c.DataSource);

            var result = await c.List(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            Assert.Equal(10, result.List.Count());
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(1, result.PageCount);

            var first = await c.Get("1");
            Assert.Equal(1, first.CompanyId);
        }
    }
}

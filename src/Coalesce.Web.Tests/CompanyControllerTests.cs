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

            var result = await c.List();
            Assert.Equal(result.List.Count(), 10);
            Assert.Equal(result.TotalCount, 10);
            Assert.Equal(result.PageCount, 1);

            var first = await c.Get("1");
            Assert.Equal(first.CompanyId, 1);
        }
    }
}

using Coalesce.Domain;
using Coalesce.Domain.External;
using Coalesce.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Web.Tests
{
    public class DevTeamControllerTests : IClassFixture<DatabaseFixtureLocalDb>
    {
        private DevTeamController _pc;

        public DevTeamControllerTests(DatabaseFixtureLocalDb dbFixture)
        {
            DbFixture = dbFixture;
            _pc = new DevTeamController();
            _pc.Db = DbFixture.Db;
        }
        private DatabaseFixtureLocalDb DbFixture { get; set; }

        [Fact]
        public async void ListGeneral()
        {
            Assert.NotNull(_pc.ReadOnlyDataSource);

            var result = await _pc.List(null, null, null, null, null, null, null, null, null, null);
            Assert.Equal(4, result.List.Count());
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(1, result.PageCount);
            var person = result.List.First();
            Assert.Equal("Office", person.Name);
        }
        [Fact]
        public async void ListPaging()
        {
            var result = await _pc.List(null, null, null, 2, 2, null, null, null, null, null);
            Assert.Equal(result.List.Count(), 2);
            Assert.Equal(result.Page, 2);
            Assert.Equal(result.TotalCount, 4);
            Assert.Equal(result.PageCount, 2);
        }

        [Fact]
        public async void ListOrderByName()
        {
            var result = await _pc.List(null, "Name", null, null, null, null, null, null, null, null);
            var person = result.List.First();
            Assert.Equal("Office", person.Name);
            Assert.Equal(4, result.List.Count());
        }

        [Fact]
        public async void Count()
        {
            var result = await _pc.Count();
            Assert.Equal(4, result);
        }

    }
}

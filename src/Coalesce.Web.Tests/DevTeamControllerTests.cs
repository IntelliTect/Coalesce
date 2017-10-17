using System.Linq;
using System.Threading.Tasks;
using Coalesce.Domain.External;
using Coalesce.Web.Api;
using Coalesce.Web.Models;
using IntelliTect.Coalesce.Models;
using Xunit;

namespace Coalesce.Web.Tests
{
    [Collection("Database collection")]
    public class DevTeamControllerTests : IClassFixture<DatabaseFixtureLocalDb>
    {
        public DevTeamControllerTests(DatabaseFixtureLocalDb dbFixture)
        {
            DbFixture = dbFixture;
            _pc = new DevTeamController();
            _pc.Db = DbFixture.Db;
        }

        private readonly DevTeamController _pc;
        private DatabaseFixtureLocalDb DbFixture { get; }

        [Fact]
        public async Task Count()
        {
            int result = await _pc.Count();
            Assert.Equal(4, result);
        }

        [Fact]
        public async Task ListGeneral()
        {
            Assert.NotNull(_pc.ReadOnlyDataSource);

            GenericListResult<DevTeam, DevTeamDtoGen> result =
                await _pc.List();
            Assert.Equal(4, result.List.Count());
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(1, result.PageCount);
            DevTeamDtoGen person = result.List.First();
            Assert.Equal("Office", person.Name);
        }

        [Fact]
        public async Task ListOrderByName()
        {
            GenericListResult<DevTeam, DevTeamDtoGen> result =
                await _pc.List(null, "Name");
            DevTeamDtoGen person = result.List.First();
            Assert.Equal("Office", person.Name);
            Assert.Equal(4, result.List.Count());
        }

        [Fact]
        public async Task ListPaging()
        {
            GenericListResult<DevTeam, DevTeamDtoGen> result =
                await _pc.List(null, null, null, 2, 2);
            Assert.Equal(2, result.List.Count());
            Assert.Equal(2, result.Page);
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(2, result.PageCount);
        }
    }
}
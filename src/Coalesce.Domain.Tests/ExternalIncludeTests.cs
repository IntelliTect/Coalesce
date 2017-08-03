using System.Linq;
using IntelliTect.Coalesce.Data;
using Xunit;

namespace Coalesce.Domain.Tests
{
    public class ExternalIncludeTests : IClassFixture<DatabaseFixture>
    {
        public ExternalIncludeTests(DatabaseFixture dbFixture)
        {
            Db = dbFixture.Db;
        }

        private readonly AppDbContext Db;

        [Fact]
        public void Mapping()
        {
            Assert.True(Db.People.Any());
            Assert.NotNull(Db.Cases.IncludeExternal(f => f.DevTeamAssigned).First().DevTeamAssigned);
            Assert.NotNull(Db.Cases.First().IncludeExternal(f => f.DevTeamAssigned).DevTeamAssigned);
        }
    }
}
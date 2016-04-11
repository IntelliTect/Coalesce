using Intellitect.ComponentModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Domain.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class ExternalIncludeTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture DbFixture { get; set; }

        public ExternalIncludeTests(DatabaseFixture dbFixture)
        {
            DbFixture = dbFixture;
        }
        [Fact]
        public void Mapping()
        {
            Assert.True(DbFixture.Db.People.Any());
            Assert.NotNull(DbFixture.Db.Cases.IncludeExternal(f => f.DevTeamAssigned).First().DevTeamAssigned);
            Assert.NotNull(DbFixture.Db.Cases.First().IncludeExternal(f => f.DevTeamAssigned).DevTeamAssigned);
        }

    }
}

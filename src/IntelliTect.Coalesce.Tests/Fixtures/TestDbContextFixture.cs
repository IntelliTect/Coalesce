using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Fixtures
{
    public class TestDbContextTests
    {
        public TestDbContextTests()
        {
            Db = new TestDbContext();
            CrudContext = new CrudContext<TestDbContext>(Db, new System.Security.Claims.ClaimsPrincipal())
            {
                ReflectionRepository = ReflectionRepositoryFactory.Reflection
            };
        }

        public TestDbContext Db { get; }
        public CrudContext<TestDbContext> CrudContext { get; private set; }
    }
}

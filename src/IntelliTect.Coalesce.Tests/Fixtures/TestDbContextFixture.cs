using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;

namespace IntelliTect.Coalesce.Tests.Fixtures
{
    public class TestDbContextFixture
    {
        public TestDbContextFixture()
        {
            Db = new TestDbContext();
            CrudContext = new CrudContext<TestDbContext>(Db, () => new System.Security.Claims.ClaimsPrincipal())
            {
                ReflectionRepository = ReflectionRepositoryFactory.Reflection
            };
        }

        public TestDbContext Db { get; }
        public CrudContext<TestDbContext> CrudContext { get; private set; }
    }
}

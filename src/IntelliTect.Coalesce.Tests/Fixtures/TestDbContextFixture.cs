using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;

namespace IntelliTect.Coalesce.Tests.Fixtures
{
    public class TestDbContextFixture
    {
        public TestDbContextFixture()
        {
            Db = new AppDbContext();
            CrudContext = new CrudContext<AppDbContext>(Db, () => new System.Security.Claims.ClaimsPrincipal())
            {
                ReflectionRepository = ReflectionRepositoryFactory.Reflection
            };
        }

        public AppDbContext Db { get; }
        public CrudContext<AppDbContext> CrudContext { get; }
    }
}

using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;

namespace IntelliTect.Coalesce.Testing.Fixtures;

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

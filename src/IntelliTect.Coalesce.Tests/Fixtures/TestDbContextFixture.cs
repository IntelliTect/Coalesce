using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.Tests.Fixtures
{
    public class TestDbContextTests : TestDbContextTests<TestDbContext>
    {

    }

    public class TestDbContextTests<TContext> where TContext :  DbContext, new()
    {
        public TestDbContextTests()
        {
            Db = new TContext();
            CrudContext = new CrudContext<TContext>(Db, new System.Security.Claims.ClaimsPrincipal(), CancellationToken.None)
            {
                ReflectionRepository = ReflectionRepositoryFactory.Reflection
            };
        }

        public TContext Db { get; }
        public CrudContext<TContext> CrudContext { get; set; }
    }
}

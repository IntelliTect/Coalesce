using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Api.Behaviors
{
    public class StandardBehaviorsTests : TestDbContextTests<AccessibleTokenContext>
    {
        public StandardBehaviorsTests()
        {
            CaseBehavior = Behavior<Case>();
        }

        public StandardBehaviors<Case, AccessibleTokenContext> CaseBehavior { get; }

        private StandardBehaviors<T, AccessibleTokenContext> Behavior<T>()
            where T : class, new()
        {
            return new StandardBehaviors<T, AccessibleTokenContext>(CrudContext);
        }

        [Fact]
        [Description("https://github.com/IntelliTect/Coalesce/issues/59")]
        public async Task SaveChanges_Cancellable()
        {
            CrudContext = new CrudContext<AccessibleTokenContext>(Db, new ClaimsPrincipal(), new CancellationToken(true));

            Db.Cancellation = ctx => Assert.True(ctx.IsCancellationRequested);

            StandardBehaviors<Case, AccessibleTokenContext> sut = Behavior<Case>();

            await sut.SaveAsync(new CaseDto(), new StandardDataSource<Case, AccessibleTokenContext>(CrudContext),
                new DataSourceParameters());
        }

        [Fact]
        [Description("https://github.com/IntelliTect/Coalesce/issues/59")]
        public async Task Delete_Cancellable()
        {

            CrudContext = new CrudContext<AccessibleTokenContext>(Db, new ClaimsPrincipal(), new CancellationToken(true));

            Db.Cancellation = ctx => Assert.True(ctx.IsCancellationRequested);

            StandardBehaviors<Case, AccessibleTokenContext> sut = Behavior<Case>();

            await sut.DeleteAsync<CaseDto>(42, new StandardDataSource<Case, AccessibleTokenContext>(CrudContext),
                new DataSourceParameters());
        }
    }

    public class AccessibleTokenContext : TestDbContext
    {
        public Action<CancellationToken> Cancellation { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Cancellation?.Invoke(cancellationToken);
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.Tests.Api.Behaviors;

public class StandardBehaviorsTests : TestDbContextFixture
{
    public StandardDataSource<Case, AppDbContext> CaseSource { get; }

    public StandardBehaviorsTests() : base()
    {
        CaseSource = Source<Case>();
    }

    private StandardDataSource<T, AppDbContext> Source<T>()
        where T : class
        => new StandardDataSource<T, AppDbContext>(CrudContext);

    private StandardBehaviors<T, AppDbContext> Behaviors<T>()
        where T : class
        => new StandardBehaviors<T, AppDbContext>(CrudContext);

    private class TransformDs : StandardDataSource<Case, AppDbContext>
    {
        public TransformDs(CrudContext<AppDbContext> context) : base(context) { }

        public override Task TransformResultsAsync(IReadOnlyList<Case> results, IDataSourceParameters parameters)
        {
            foreach (var result in results)
            {
                // Normally you would never transform a DB column here,
                // but we need something obvious and easily assertable
                result.Title = "TRANSFORMED";
            }
            return Task.CompletedTask;
        }
    }

    [Test]
    public async Task Save_HydratesResultWithDataSourceTransformAsync()
    {
        var ds = new TransformDs(CrudContext)
            .AddModel(m => m.Title, "original title");

        var dto = new TestDto<Case>(1, c => { c.Description = "new desc"; });
        var result = await Behaviors<Case>().SaveAsync<TestDto<Case>, TestDto<Case>>(dto, ds, new DataSourceParameters());

        result.AssertSuccess();
        await Assert.That(result.Object.SourceEntity.Description).IsEqualTo("new desc");
        await Assert.That(result.Object.SourceEntity.Title).IsEqualTo("TRANSFORMED");
    }


    private class UntrackedDs : StandardDataSource<Case, AppDbContext>
    {
        public UntrackedDs(CrudContext<AppDbContext> context) : base(context) { }
        public override IQueryable<Case> GetQuery(IDataSourceParameters parameters) => base.GetQuery(parameters).AsNoTracking();
    }

    [Test]
    public async Task Save_WhenDataSourceIsUntracked_RetracksEntityAndSaves()
    {
        // Arrange
        var ds = new UntrackedDs(CrudContext)
            .AddModel(new Case { CaseKey = 1, Description = "bob" });
        ds.Db.ChangeTracker.Clear();

        // Act
        var dto = new TestDto<Case>(1, c => { c.Description = "new desc"; });
        var result = await Behaviors<Case>().SaveAsync<TestDto<Case>, TestDto<Case>>(dto, ds, new DataSourceParameters());

        // Assert2: Entity saved as expected
        ds.Db.ChangeTracker.Clear();
        await Assert.That(ds.Db.Cases.Single().Description).IsEqualTo("new desc");
    }

    [Test]
    public async Task DetermineSaveKindAsync_CanDeterminePkFromEfEntity()
    {
        // Arrange
        var behaviors = Behaviors<RequiredAndInitModel>();

        // Act
        var result = await behaviors.DetermineSaveKindAsync(new RequiredAndInitModelParameterDto { Id = 42 }, Source<RequiredAndInitModel>(), new DataSourceParameters());

        // Assert
        await Assert.That(result).IsEqualTo((SaveKind.Update, 42));
    }

    class RequiredAndInitModelParameterDto : IParameterDto<RequiredAndInitModel>
    {
        public int Id { get; set; }

        public void MapTo(RequiredAndInitModel obj, IMappingContext context) => throw new System.NotImplementedException();

        public RequiredAndInitModel MapToNew(IMappingContext context) => throw new System.NotImplementedException();
    }

    [Test]
    public async Task DetermineSaveKindAsync_CanDeterminePkFromStandaloneEntity()
    {
        // Arrange
        var behaviors = Behaviors<StandaloneReadWrite>();

        // Act
        var result = await behaviors.DetermineSaveKindAsync(new StandaloneReadWriteDto { Id = 42 }, Source<StandaloneReadWrite>(), new DataSourceParameters());

        // Assert
        await Assert.That(result).IsEqualTo((SaveKind.Update, 42));
    }

    class StandaloneReadWriteDto : SparseDto, IGeneratedParameterDto<StandaloneReadWrite>
    {
        public int Id { get; set; }

        public void MapTo(StandaloneReadWrite obj, IMappingContext context) => throw new System.NotImplementedException();

        public StandaloneReadWrite MapToNew(IMappingContext context) => throw new System.NotImplementedException();
    }

    [Test]
    public async Task DeleteAsync_ReturnsSuccessAndNull()
    {
        // Arrange
        var behaviors = Behaviors<Case>();
        var ds = Source<Case>()
            .AddModel(new Case { CaseKey = 1, Description = "bob" });
        Db.ChangeTracker.Clear();

        // Act
        var result = await behaviors.DeleteAsync<TestDto<Case>>(1, ds, new DataSourceParameters());

        // Assert
        await Assert.That(result.WasSuccessful).IsTrue();
        await Assert.That(result.Object).IsNull();
    }

    [Test]
    public async Task DeleteAsync_AfterDeleteFailure_ReturnsFailure()
    {
        // Arrange
        var mock = new Mock<StandardBehaviors<Case, AppDbContext>>(CrudContext);
        mock.CallBase = true;
        mock.Setup(b => b.AfterDeleteAsync(It.IsAny<Case>())).ReturnsAsync("AfterDeleteFailure");
        var behaviors = mock.Object;

        var ds = Source<Case>()
            .AddModel(new Case { CaseKey = 1, Description = "bob" });
        Db.ChangeTracker.Clear();

        // Act
        var result = await behaviors.DeleteAsync<TestDto<Case>>(1, ds, new DataSourceParameters());

        // Assert
        await Assert.That(result.WasSuccessful).IsFalse();
        await Assert.That(result.Message).IsEqualTo("AfterDeleteFailure");
    }

    [Test]
    public async Task DeleteAsync_AfterDeleteReturnsItem_ReturnsItem()
    {
        var expectedResult = new ItemResult<Case>(new Case { Description = "Deleted" }, new IncludeTree());

        // Arrange
        var mock = new Mock<StandardBehaviors<Case, AppDbContext>>(CrudContext);
        mock.CallBase = true;
        mock.Setup(b => b.AfterDeleteAsync(It.IsAny<Case>())).ReturnsAsync(expectedResult);
        var behaviors = mock.Object;

        var ds = Source<Case>()
            .AddModel(new Case { CaseKey = 1, Description = "bob" });
        Db.ChangeTracker.Clear();

        // Act
        var result = await behaviors.DeleteAsync<TestDto<Case>>(1, ds, new DataSourceParameters());

        // Assert
        await Assert.That(result.WasSuccessful).IsTrue();
        await Assert.That(result.Object.SourceEntity.Description).IsEqualTo("Deleted");
        await Assert.That(result.Object.SourceIncludeTree).IsEqualTo(expectedResult.IncludeTree);
    }
}
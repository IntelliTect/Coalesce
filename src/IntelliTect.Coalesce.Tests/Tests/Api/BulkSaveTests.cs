#nullable enable

using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace IntelliTect.Coalesce.Tests.Api;

public class BulkSaveTests : IDisposable
{
    // SQLite, unlike the in-memory provider, enforces unique indexes and foreign keys.
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly IServiceProvider _services;
    private readonly ClaimsPrincipal _user = new(new ClaimsIdentity("Test"));

    public BulkSaveTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _db.Database.ExecuteSqlRaw("""CREATE UNIQUE INDEX "IX_CaseProduct_Pair" ON "CaseProduct" ("CaseId", "ProductId")""");

        var crudContext = new CrudContext<AppDbContext>(_db, () => _user)
        {
            ReflectionRepository = ReflectionRepositoryFactory.Reflection
        };

        var services = new ServiceCollection();
        services.AddCoalesce(b => b.AddContext<AppDbContext>());
        _services = services
            .AddSingleton(_db)
            .AddSingleton(crudContext)
            .AddSingleton<CrudContext>(crudContext)
            .BuildServiceProvider();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Test]
    public async Task BulkSave_DeleteAndReinsertWithSameUniqueKey_Succeeds()
    {
        var caseId = Seed(new Case { Title = "c" }).CaseKey;
        var productId = Seed(new Product { Name = "p" }).ProductId;
        var oldId = Seed(new CaseProduct { CaseId = caseId, ProductId = productId }).CaseProductId;

        var result = await BulkSave(
            Item<CaseProduct, CaseProductDto>("delete", new() { CaseProductId = oldId }),
            Item<CaseProduct, CaseProductDto>("save", new() { CaseId = caseId, ProductId = productId })
        );

        await Assert.That(result.WasSuccessful).IsTrue().Because(result.Message ?? "");
        var remaining = await _db.CaseProducts.AsNoTracking().SingleAsync();
        await Assert.That(remaining.CaseProductId).IsNotEqualTo(oldId);
        await Assert.That(remaining.CaseId).IsEqualTo(caseId);
        await Assert.That(remaining.ProductId).IsEqualTo(productId);
    }

    [Test]
    public async Task BulkSave_DeletePrincipalAfterSaveRemovesReference_Succeeds()
    {
        var companyId = Seed(new Company { Name = "co" }).CompanyId;
        var personId = Seed(new Person { FirstName = "p", CompanyId = companyId }).PersonId;
        var caseId = Seed(new Case { Title = "c", AssignedToId = personId }).CaseKey;

        var result = await BulkSave(
            Item<Person, PersonDto>("delete", new() { PersonId = personId }),
            Item<Case, CaseDto>("save", new() { CaseKey = caseId, Title = "c", AssignedToId = null })
        );

        await Assert.That(result.WasSuccessful).IsTrue().Because(result.Message ?? "");
        await Assert.That(await _db.People.AnyAsync()).IsFalse();
        var savedCase = await _db.Cases.AsNoTracking().SingleAsync();
        await Assert.That(savedCase.AssignedToId).IsNull();
    }

    private T Seed<T>(T entity) where T : class
    {
        _db.Add(entity);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
        return entity;
    }

    private async Task<ItemResult> BulkSave(params BulkSaveRequestItem[] items)
    {
        var controller = new TestController(_services.GetRequiredService<CrudContext<AppDbContext>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user, RequestServices = _services }
            }
        };

        var repo = ReflectionRepositoryFactory.Reflection;
        return await controller.BulkSave(
            new BulkSaveRequest { Items = [.. items] },
            new DataSourceFactory(_services, repo),
            new BehaviorsFactory(_services, repo)
        );
    }

    private static BulkSaveRequestItem Item<T, TDto>(string action, TDto dto)
        where T : class
        where TDto : class, IParameterDto<T>, new()
    {
        var repo = ReflectionRepositoryFactory.Reflection;
        return new BulkSaveRequestItem<T, TDto>
        {
            Type = typeof(T).Name,
            Action = action,
            Data = dto,
            ClassViewModel = repo.GetClassViewModel<T>()!,
            ParamDtoClassViewModel = repo.GetClassViewModel<TDto>()!,
        };
    }

    private class TestController(CrudContext<AppDbContext> context)
        : BaseApiController<CaseProduct, CaseProductDto, CaseProductDto, AppDbContext>(context)
    {
        public Task<ItemResult<CaseProductDto>> BulkSave(
            BulkSaveRequest dto,
            IDataSourceFactory dataSourceFactory,
            IBehaviorsFactory behaviorsFactory
        ) => BulkSaveImplementation(
            dto,
            new DataSourceParameters(),
            dataSourceFactory.GetDefaultDataSource<CaseProduct>(EntityClassViewModel),
            dataSourceFactory,
            behaviorsFactory);
    }

    private abstract class TestDtoBase<T> : IGeneratedParameterDto<T>, IResponseDto<T>
        where T : class
    {
        public ISet<string> ChangedProperties => GetType().GetProperties().Select(p => p.Name).ToHashSet();
        public abstract void MapTo(T obj, IMappingContext context);
        public void MapFrom(T obj, IMappingContext context, IncludeTree? tree = null) { }
    }

    private class CaseProductDto : TestDtoBase<CaseProduct>
    {
        public int? CaseProductId { get; set; }
        public int? CaseId { get; set; }
        public int? ProductId { get; set; }

        public override void MapTo(CaseProduct obj, IMappingContext context)
        {
            if (CaseId is int caseId) obj.CaseId = caseId;
            if (ProductId is int productId) obj.ProductId = productId;
        }
    }

    private class PersonDto : TestDtoBase<Person>
    {
        public int? PersonId { get; set; }
        public override void MapTo(Person obj, IMappingContext context) { }
    }

    private class CaseDto : TestDtoBase<Case>
    {
        public int? CaseKey { get; set; }
        public string? Title { get; set; }
        public int? AssignedToId { get; set; }

        public override void MapTo(Case obj, IMappingContext context)
        {
            obj.Title = Title;
            obj.AssignedToId = AssignedToId;
        }
    }
}

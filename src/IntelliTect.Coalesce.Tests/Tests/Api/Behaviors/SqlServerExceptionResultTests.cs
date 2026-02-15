using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IntelliTect.Coalesce.Tests.Tests.Api.Behaviors;

public class SqlServerExceptionResultTests : IDisposable
{
    public SqlServerExceptionResultTests()
    {
        Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer()
            .Options
        );
        CrudContext = new CrudContext<AppDbContext>(Db, () => new System.Security.Claims.ClaimsPrincipal())
        {
            ReflectionRepository = ReflectionRepositoryFactory.Reflection
        };
    }

    public void Dispose()
    {
        Db?.Dispose();
    }

    public AppDbContext Db { get; }
    public CrudContext<AppDbContext> CrudContext { get; }

    private StandardBehaviors<T, AppDbContext> Behaviors<T>()
        where T : class, new()
        => new StandardBehaviors<T, AppDbContext>(CrudContext);

    [Test]
    public async Task InsertFkConflict()
    {
        Db.Add(new CaseProduct { CaseId = 1, ProductId = 42 });

        var exception = CreateException(
            "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_CaseProduct_Product_ProductId\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.Product\", column 'ProductId'.");

        var result = Behaviors<CaseProduct>()
            .GetExceptionResult(exception, new TestSparseDto<CaseProduct>() { ChangedProperties = { "ProductId" } });

        await result.AssertError("The value of Product is not valid.");
    }

    [Test]
    public async Task UpdateFkConflict()
    {
        var entry = Db.Entry(new CaseProduct { CaseId = 1, ProductId = 42 });
        entry.State = EntityState.Unchanged;
        entry.Property("ProductId").IsModified = true;

        var exception = CreateException(
            "The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_CaseProduct_Product_ProductId\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.Product\", column 'ProductId'.");

        var result = Behaviors<CaseProduct>()
            .GetExceptionResult(exception, new TestSparseDto<CaseProduct>() { ChangedProperties = { "ProductId" } });

        await result.AssertError("The value of Product is not valid.");
    }

    [Test]
    public async Task FkConstraint_WhenPropIsNotUserChanged_DoesNotProduceFriendlyError()
    {
        var entry = Db.Entry(new CaseProduct { CaseId = 1, ProductId = 42 });
        entry.State = EntityState.Unchanged;
        entry.Property("ProductId").IsModified = true;

        var exception = CreateException(
            "The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_CaseProduct_Product_ProductId\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.Product\", column 'ProductId'.");

        var result = Behaviors<CaseProduct>()
            .GetExceptionResult(exception, new TestSparseDto<CaseProduct>() { ChangedProperties = { /* empty */ } });

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DeleteFkConflict()
    {
        var entry = Db.Entry(new Product { ProductId = 42 });
        entry.State = EntityState.Deleted;

        var exception = CreateException(
            "The DELETE statement conflicted with the REFERENCE constraint \"FK_CaseProduct_Product_ProductId\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.CaseProduct\", column 'ProductId'.");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, null);

        await result.AssertError("The Product is still referenced by at least one Case Product.");
    }

    [Test]
    public async Task FkConstraint_UnknownTableName()
    {
        Db.Add(new CaseProduct { CaseId = 1, ProductId = 42 });

        var exception = CreateException(
            "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_CaseProduct_Product_ProductId\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.FooTable\", column 'ProductId'.");

        var result = Behaviors<CaseProduct>().GetExceptionResult(exception, new TestSparseDto<CaseProduct>());

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task FkConstraint_UnknownColumnName()
    {
        Db.Add(new CaseProduct { CaseId = 1, ProductId = 42 });

        var exception = CreateException(
            "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_CaseProduct_Product_ProductId\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.Product\", column 'FooColumn'.");

        var result = Behaviors<CaseProduct>().GetExceptionResult(exception, new TestSparseDto<CaseProduct>());

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task FkConstraint_UnknownConstraintName()
    {
        Db.Add(new CaseProduct { CaseId = 1, ProductId = 42 });

        var exception = CreateException(
            "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_FooFk\". " +
            "The conflict occurred in database \"FooDb\", table \"dbo.Product\", column 'ProductId'.");

        var result = Behaviors<CaseProduct>().GetExceptionResult(exception, new TestSparseDto<CaseProduct>());

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task UniqueIndexConflict()
    {
        Db.Add(new Product { UniqueId1 = "qwerty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_UniqueId1'. " +
            "The duplicate key value is (qwerty)");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { "UniqueId1" } });

        await result.AssertError("A different item with ID1 'qwerty' already exists.");
    }

    [Test]
    public async Task UniqueIndexConflict_WhenPropIsNotUserChanged_DoesNotProduceFriendlyError()
    {
        Db.Add(new Product { UniqueId1 = "qwerty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_UniqueId1'. " +
            "The duplicate key value is (qwerty)");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { /* empty */ } });

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task UniqueIndexConflict_MultiPropIndex()
    {
        Db.Add(new Product { UniqueId1 = "qwe, rty", UniqueId2 = "foo,bar" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_UniqueId1_UniqueId2'. " +
            "The duplicate key value is (qwe, rty, foo,bar)");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { "UniqueId1", "UniqueId2" } });

        await result.AssertError("A different item with ID1 'qwe, rty' and ID2 'foo,bar' already exists.");
    }

    [Test]
    public async Task UniqueIndexConflict_HandlesNull()
    {
        Db.Add(new Product { UniqueId1 = "qwe, rty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_UniqueId1_UniqueId2'. " +
            "The duplicate key value is (qwe, rty, <NULL>)");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { "UniqueId1" } });

        await result.AssertError("A different item with ID1 'qwe, rty' and ID2 <NULL> already exists.");
    }

    [Test]
    public async Task UniqueIndexConflict_MultiPropIndex_PartiallyChanged()
    {
        Db.Add(new Product { UniqueId1 = "qwe, rty", UniqueId2 = "foo,bar" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_UniqueId1_UniqueId2'. " +
            "The duplicate key value is (qwe, rty, foo,bar)");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { "UniqueId2" } });

        await result.AssertError("A different item with ID1 'qwe, rty' and ID2 'foo,bar' already exists.");
    }

    [Test]
    public async Task UniqueIndexConflict_PartiallyInternalMultiPropIndex_ReportsNonInternalParts()
    {
        Db.Add(new Product { TenantId = 1, UniqueId1 = "qwerty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_TenantId_UniqueId1'. " +
            "The duplicate key value is (1, qwerty)");

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { "UniqueId1" } });

        // Doesn't report the tenantID, which is internal.
        await result.AssertError("A different item with ID1 'qwerty' already exists.");
    }

    [Test]
    public async Task UniqueIndexConflict_UnknownTableName()
    {
        Db.Add(new Product { TenantId = 1, UniqueId1 = "qwerty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.FooBar' with unique index 'IX_Product_TenantId_UniqueId1'. " +
            "The duplicate key value is (1, qwerty)");

        var result = Behaviors<Product>().GetExceptionResult(exception, new TestSparseDto<Product>());

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task UniqueIndexConflict_UnknownIndex()
    {
        Db.Add(new Product { TenantId = 1, UniqueId1 = "qwerty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_FooIndex'. " +
            "The duplicate key value is (1, qwerty)");

        var result = Behaviors<Product>().GetExceptionResult(exception, new TestSparseDto<Product>());

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DbContextIsDisposed()
    {
        Db.Add(new Product { UniqueId1 = "qwerty" });

        var exception = CreateException(
            "Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_UniqueId1'. " +
            "The duplicate key value is (qwerty)");

        // The context might dispose before GetExceptionResult is reached if the user is using an IDbContextFactory
        // with a `using` inside their ExecuteSaveAsync implementation.
        Db.Dispose();

        var result = Behaviors<Product>()
            .GetExceptionResult(exception, new TestSparseDto<Product>() { ChangedProperties = { "UniqueId1" } });

        await result.AssertError("A different item with ID1 'qwerty' already exists.");
    }

    private DbUpdateException CreateException(string error)
    {
        return new DbUpdateException("", CreateSqlException(547, error), Db.ChangeTracker.Entries().ToList());
    }

    private static SqlException CreateSqlException(int errorCode, string errorMessage)
    {
        // AI-generated
        // Use reflection to create a SqlError instance
        var sqlError = CreateSqlError(errorCode, errorMessage);

        // Create an SqlErrorCollection and add the SqlError instance to it
        var errorCollection = CreateSqlErrorCollection(sqlError);

        // Call the internal static CreateException method on SqlException
        return CreateSqlException(errorCollection);
    }

    private static SqlError CreateSqlError(int errorCode, string errorMessage)
    {
        // (int infoNumber, byte errorState, byte errorClass, string server, string message, string procedure, int lineNumber, uint win32ErrorCode, Exception exception = null)
        var sqlErrorCtor = typeof(SqlError).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];

        var type = sqlErrorCtor.GetParameters()[7].ParameterType;
        return (SqlError)sqlErrorCtor.Invoke([
            errorCode,
            (byte)0,
            (byte)0,
            "Server",
            errorMessage,
            "Procedure",
            0,
            sqlErrorCtor.GetParameters()[7].ParameterType == typeof(uint) ? (object)(uint)0 : (object)(int)0,
            null]);
    }

    private static SqlErrorCollection CreateSqlErrorCollection(SqlError sqlError)
    {
        // AI-generated
        // Create an empty SqlErrorCollection instance using reflection
        var errorCollection = Activator.CreateInstance(typeof(SqlErrorCollection), true);

        // Use reflection to add the SqlError instance to the SqlErrorCollection
        var method = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(errorCollection, new object[] { sqlError });

        return (SqlErrorCollection)errorCollection;
    }

    private static SqlException CreateSqlException(SqlErrorCollection errorCollection)
    {
        // AI-generated
        // Use reflection to invoke the internal static method CreateException(SqlErrorCollection, string)
        var sqlExceptionType = typeof(SqlException);
        var createExceptionMethod = sqlExceptionType.GetMethod("CreateException", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(SqlErrorCollection), typeof(string) }, null);

        // Call the method and return the SqlException
        return (SqlException)createExceptionMethod.Invoke(null, new object[] { errorCollection, "11.0.0" });  // SQL Server version is arbitrary
    }
}

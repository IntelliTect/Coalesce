using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using TUnit.Assertions.Enums;

namespace IntelliTect.Coalesce.Tests.Api;

public class BulkSaveRequestTests
{
    [Test]
    public async Task PartitionDeletes_EmptyRequest_ReturnsNothing()
    {
        var (early, late) = Request().PartitionDeletes();

        await Assert.That(early).IsEmpty();
        await Assert.That(late).IsEmpty();
    }

    [Test]
    public async Task PartitionDeletes_NoSaves_DeletesFirstInRequestOrder()
    {
        var deleteProduct = Item<CaseProduct>("delete");
        var deleteCase = Item<Case>("delete");
        var deletePerson = Item<Person>("delete");
        var request = Request(deleteProduct, deleteCase, deletePerson);

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEquivalentTo([deleteProduct, deleteCase, deletePerson], CollectionOrdering.Matching);
        await Assert.That(late).IsEmpty();
    }

    [Test]
    public async Task PartitionDeletes_DeleteAndReinsertOfSameType_DeletesFirst()
    {
        var delete = Item<CaseProduct>("delete");
        var request = Request(delete, Item<CaseProduct>("save"));

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEquivalentTo([delete]);
        await Assert.That(late).IsEmpty();
    }

    [Test]
    public async Task PartitionDeletes_SavedTypeReferencesDeletedType_DeletesLast()
    {
        var delete = Item<Person>("delete");
        var request = Request(delete, Item<Case>("save"));

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEmpty();
        await Assert.That(late).IsEquivalentTo([delete]);
    }

    [Test]
    public async Task PartitionDeletes_PrincipalOfLateDelete_AlsoDeletesLast()
    {
        var deletePerson = Item<Person>("delete");
        var deleteCompany = Item<Company>("delete");
        var deleteProduct = Item<CaseProduct>("delete");
        var request = Request(deleteCompany, deletePerson, deleteProduct, Item<Case>("save"));

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEquivalentTo([deleteProduct]);
        await Assert.That(late).IsEquivalentTo([deleteCompany, deletePerson], CollectionOrdering.Matching);
    }

    [Test]
    public async Task PartitionDeletes_PreservesRequestOrderWithinEachGroup()
    {
        var deleteProduct1 = Item<CaseProduct>("delete");
        var deletePerson1 = Item<Person>("delete");
        var deleteProduct2 = Item<CaseProduct>("delete");
        var deletePerson2 = Item<Person>("delete");
        var request = Request(deleteProduct1, deletePerson1, Item<Case>("save"), deleteProduct2, deletePerson2);

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEquivalentTo([deleteProduct1, deleteProduct2], CollectionOrdering.Matching);
        await Assert.That(late).IsEquivalentTo([deletePerson1, deletePerson2], CollectionOrdering.Matching);
    }

    [Test]
    public async Task PartitionDeletes_SelfReferencingType_DeletesLast()
    {
        var delete1 = Item<RecursiveHierarchy>("delete");
        var delete2 = Item<RecursiveHierarchy>("delete");
        var request = Request(delete1, Item<RecursiveHierarchy>("save"), delete2);

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEmpty();
        await Assert.That(late).IsEquivalentTo([delete1, delete2], CollectionOrdering.Matching);
    }

    [Test]
    public async Task PartitionDeletes_SelfReferencingTypeWithUnrelatedSave_DeletesFirst()
    {
        var delete1 = Item<RecursiveHierarchy>("delete");
        var delete2 = Item<RecursiveHierarchy>("delete");
        var request = Request(delete1, delete2, Item<Case>("save"));

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEquivalentTo([delete1, delete2], CollectionOrdering.Matching);
        await Assert.That(late).IsEmpty();
    }

    [Test]
    public async Task PartitionDeletes_DeletedTypeDerivesFromForeignKeyPrincipal_DeletesLast()
    {
        // AbstractModelPerson.AbstractModelId references the base type.
        var deleteImpl = Item<AbstractImpl2>("delete");
        var request = Request(deleteImpl, Item<AbstractModelPerson>("save"));

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEmpty();
        await Assert.That(late).IsEquivalentTo([deleteImpl]);
    }

    [Test]
    public async Task PartitionDeletes_CustomDtoSave_UsesEntityForeignKeys()
    {
        var delete = Item<Person>("delete");
        var customDtoSave = new TestItem
        {
            Action = "save",
            ClassViewModel = ReflectionRepositoryFactory.Reflection.GetClassViewModel<CaseDtoStandalone>()!.BaseViewModel,
        };
        var request = Request(delete, customDtoSave);

        var (early, late) = request.PartitionDeletes();

        await Assert.That(early).IsEmpty();
        await Assert.That(late).IsEquivalentTo([delete]);
    }

    private static BulkSaveRequest Request(params BulkSaveRequestItem[] items) => new() { Items = [.. items] };

    private static BulkSaveRequestItem Item<T>(string action) => new TestItem
    {
        Action = action,
        ClassViewModel = ReflectionRepositoryFactory.Reflection.GetClassViewModel<T>()!,
    };

    private class TestItem : BulkSaveRequestItem
    {
        internal override void Initialize(IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory)
            => throw new NotSupportedException();
        internal override Task<SaveKind> DetermineSaveKindAsync(DataSourceParameters parameters)
            => throw new NotSupportedException();
        internal override Task<ItemResult> SaveAsync(DataSourceParameters parameters)
            => throw new NotSupportedException();
        internal override Task<ItemResult> DeleteAsync(DataSourceParameters parameters)
            => throw new NotSupportedException();
    }
}

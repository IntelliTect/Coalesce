using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Testing.TargetClasses;
using System.Security.Claims;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Mapping;

namespace IntelliTect.Coalesce.Tests.Tests.Security;

public class PropertySecurityTests
{
    [Test]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaRead))]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaReadOnly))]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaReadOnlyApi))]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaNonPublicSetter))]
    public async Task ReadOnly_CorrectForReadOnlyProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsReadOnly).IsTrue();
        await Assert.That(prop.IsClientSerializable).IsFalse();
        await Assert.That(prop.IsClientWritable).IsFalse();
    }

    [Test]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadWriteDifferentRoles))]
    public async Task ReadWriteDifferentExplicitRoles_RequiresBothRolesForEdit(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;
        await prop.SecurityInfo.Edit.RoleLists.AssertCollection(
            async l => await Assert.That(l).IsEquivalentTo(new[] { "ReadRole" }),
            async l => await Assert.That(l).IsEquivalentTo(new[] { "EditRole" })
        );

        await Assert.That(prop.SecurityInfo.Edit.IsAllowed(new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "ReadRole"),
            new Claim(ClaimTypes.Role, "EditRole"),
        })))).IsTrue();

        await Assert.That(prop.SecurityInfo.Edit.IsAllowed(new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "EditRole"),
        })))).IsFalse();
    }

    [Test]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ExternalTypeUsageOfEntityWithInternalDbSet))]
    public async Task UsageOfEntityWithInternalDbSet_IsTreatedLikeExternalType(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsDbMapped).IsFalse();
        await Assert.That(prop.Object.IsDbMappedType).IsFalse();
        await Assert.That(prop.Object.HasDbSet).IsFalse();
        await Assert.That(prop.Object.DbContext).IsNull();
        await Assert.That(prop.Object.DbContextUsage).IsNull();

        await Assert.That(prop.SecurityInfo.Read.IsAllowed()).IsTrue();

        // Both PropSec and DbSetIsInternalUse are considered external types,
        // so the property should be accepted as input
        await Assert.That(prop.SecurityInfo.Edit.IsAllowed()).IsTrue();
    }

    [Test]
    [PropertyViewModelData<PropSec>(nameof(PropSec.RestrictMultiple))]
    public async Task MappingRestrictions_AreDiscovered(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;
        await prop.SecurityInfo.Restrictions.AssertCollection(
            async r => await Assert.That(r.Name).IsEqualTo(typeof(AuthenticatedRestriction).Name),
            async r => await Assert.That(r.Name).IsEqualTo(typeof(Restrict2).Name)
        );
    }

    [Test]
    [ReflectionPropertyViewModelData<PropSec>(nameof(PropSec.RestrictFilter))]
    public async Task MappingRestrictions_UserCanFilter_RespectsRestrictions(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.SecurityInfo.IsFilterAllowed(new MappingContext(new ClaimsPrincipal(), null))).IsFalse();
        await Assert.That(prop.SecurityInfo.IsFilterAllowed(new MappingContext(new ClaimsPrincipal(new ClaimsIdentity("foo")), null))).IsTrue();
    }

    [Test]
    [ReflectionClassViewModelData(typeof(ComplexModel))]
    public async Task IncludeChildren_IncludesNonSuppressedMembers(ClassViewModelData data)
    {
        ClassViewModel vm = data;
        const string propName = nameof(ComplexModel.ReferenceNavigation);

        var tree = IncludeTree.For<ComplexModel>(q => q.IncludeChildren(vm.ReflectionRepository));
        var prop = vm.PropertyByName(propName);

        // Precondition:
        await Assert.That(prop.Role).IsEqualTo(PropertyRole.ReferenceNavigation);

        // Assert
        await Assert.That(prop.CanAutoInclude).IsTrue();
        await Assert.That(tree[propName]).IsNotNull();
    }

    [Test]
    [ReflectionClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.NoAutoIncludeReferenceNavigation))]
    [ReflectionClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.NoAutoIncludeByClassReferenceNavigation))]
    public async Task IncludeChildren_DoesNotIncludedOptedOutMember(ClassViewModelData data, string propName)
    {
        ClassViewModel vm = data;

        var tree = IncludeTree.For<ComplexModel>(q => q.IncludeChildren(vm.ReflectionRepository));
        var prop = vm.PropertyByName(propName);

        // Precondition:
        await Assert.That(prop.Role).IsEqualTo(PropertyRole.ReferenceNavigation);

        // Assert
        await Assert.That(prop.CanAutoInclude).IsFalse();
        await Assert.That(tree[propName]).IsNull();
    }
}

using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using IntelliTect.Coalesce.Tests.TargetClasses;
using System.Security.Claims;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using System.Linq.Expressions;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Mapping;

namespace IntelliTect.Coalesce.Tests.Tests.Security
{
    public class PropertySecurityTests
    {
        [Theory]
        [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaRead))]
        [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaReadOnly))]
        [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaReadOnlyApi))]
        [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaNonPublicSetter))]
        public void ReadOnly_CorrectForReadOnlyProps(PropertyViewModelData data)
        {
            PropertyViewModel prop = data;

            Assert.True(prop.IsReadOnly);
            Assert.False(prop.IsClientSerializable);
            Assert.False(prop.IsClientWritable);
        }

        [Theory]
        [PropertyViewModelData<PropSec>(nameof(PropSec.ReadWriteDifferentRoles))]
        public void ReadWriteDifferentExplicitRoles_RequiresBothRolesForEdit(PropertyViewModelData data)
        {
            PropertyViewModel prop = data;

            Assert.Collection(prop.SecurityInfo.Edit.RoleLists,
                l => Assert.Equal(new[] { "ReadRole" }, l),
                l => Assert.Equal(new[] { "EditRole" }, l)
            );

            Assert.True(prop.SecurityInfo.Edit.IsAllowed(new (new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "ReadRole"),
                new Claim(ClaimTypes.Role, "EditRole"),
            }))));

            Assert.False(prop.SecurityInfo.Edit.IsAllowed(new (new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "EditRole"),
            }))));
        }

        [Theory]
        [PropertyViewModelData<PropSec>(nameof(PropSec.ExternalTypeUsageOfEntityWithInternalDbSet))]
        public void UsageOfEntityWithInternalDbSet_IsTreatedLikeExternalType(PropertyViewModelData data)
        {
            PropertyViewModel prop = data;

            Assert.False(prop.IsDbMapped);
            Assert.False(prop.Object.IsDbMappedType);
            Assert.False(prop.Object.HasDbSet);
            Assert.Null(prop.Object.DbContext);
            Assert.Null(prop.Object.DbContextUsage);

            Assert.True(prop.SecurityInfo.Read.IsAllowed());

            // Both PropSec and DbSetIsInternalUse are considered external types,
            // so the property should be accepted as input
            Assert.True(prop.SecurityInfo.Edit.IsAllowed());
        }

        [Theory]
        [PropertyViewModelData<PropSec>(nameof(PropSec.RestrictMultiple))]
        public void MappingRestrictions_AreDiscovered(PropertyViewModelData data)
        {
            PropertyViewModel prop = data;

            Assert.Collection(prop.SecurityInfo.Restrictions,
                r =>
                {
                    Assert.Equal(typeof(AuthenticatedRestriction).Name, r.Name);
                },
                r =>
                {
                    Assert.Equal(typeof(Restrict2).Name, r.Name);
                });
        }

        [Theory]
        [ReflectionPropertyViewModelData<PropSec>(nameof(PropSec.RestrictFilter))]
        public void MappingRestrictions_UserCanFilter_RespectsRestrictions(PropertyViewModelData data)
        {
            PropertyViewModel prop = data;

            Assert.False(prop.SecurityInfo.IsFilterAllowed(new MappingContext(new ClaimsPrincipal(), null)));
            Assert.True(prop.SecurityInfo.IsFilterAllowed(new MappingContext(new ClaimsPrincipal(new ClaimsIdentity("foo")), null)));
        }

        [Theory]
        [ReflectionClassViewModelData(typeof(ComplexModel))]
        public void IncludeChildren_IncludesNonSuppressedMembers(ClassViewModelData data)
        {
            ClassViewModel vm = data;
            const string propName = nameof(ComplexModel.ReferenceNavigation);

            var tree = IncludeTree.For<ComplexModel>(q => q.IncludeChildren(vm.ReflectionRepository));
            var prop = vm.PropertyByName(propName);

            // Precondition:
            Assert.Equal(PropertyRole.ReferenceNavigation, prop.Role);

            // Assert
            Assert.True(prop.CanAutoInclude);
            Assert.NotNull(tree[propName]);
        }

        [Theory]
        [ReflectionClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.NoAutoIncludeReferenceNavigation))]
        [ReflectionClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.NoAutoIncludeByClassReferenceNavigation))]
        public void IncludeChildren_DoesNotIncludedOptedOutMember(ClassViewModelData data, string propName)
        {
            ClassViewModel vm = data;

            var tree = IncludeTree.For<ComplexModel>(q => q.IncludeChildren(vm.ReflectionRepository));
            var prop = vm.PropertyByName(propName);

            // Precondition:
            Assert.Equal(PropertyRole.ReferenceNavigation, prop.Role);

            // Assert
            Assert.False(prop.CanAutoInclude);
            Assert.Null(tree[propName]);
        }
    }
}

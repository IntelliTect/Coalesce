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

namespace IntelliTect.Coalesce.Tests.Tests.Security
{
    public class PropertySecurityTests
    {
        [Theory]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaRead))]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaReadOnly))]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaReadOnlyApi))]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaNonPublicSetter))]
        public void ReadOnly_CorrectForReadOnlyProps(ClassViewModelData data, string propName)
        {
            ClassViewModel vm = data;
            var prop = vm.PropertyByName(propName);

            Assert.True(prop.IsReadOnly);
            Assert.False(prop.IsClientSerializable);
            Assert.False(prop.IsClientWritable);
        }

        [Theory]
        [ClassViewModelData(typeof(PropSec))]
        public void ReadWriteDifferentExplicitRoles_RequiresBothRolesForEdit(ClassViewModelData data)
        {
            ClassViewModel vm = data;
            var prop = vm.PropertyByName(nameof(PropSec.ReadWriteDifferentRoles));

            Assert.Collection(prop.SecurityInfo.Edit.RoleLists,
                l => Assert.Equal(new[] { "ReadRole" }, l),
                l => Assert.Equal(new[] { "EditRole" }, l)
            );

            Assert.True(prop.SecurityInfo.IsEditAllowed(new (new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "ReadRole"),
                new Claim(ClaimTypes.Role, "EditRole"),
            }))));

            Assert.False(prop.SecurityInfo.IsEditAllowed(new (new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "EditRole"),
            }))));
        }

        [Theory]
        [ClassViewModelData(typeof(PropSec))]
        public void UsageOfEntityWithInternalDbSet_IsTreatedLikeExternalType(ClassViewModelData data)
        {
            ClassViewModel vm = data;
            var prop = vm.PropertyByName(nameof(PropSec.ExternalTypeUsageOfEntityWithInternalDbSet));

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

using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class MethodViewModelTests
    {
        [Theory]
        [ClassViewModelData(
            typeof(TargetClasses.Github31.Person),
            nameof(TargetClasses.Github31.Person.GetMyPeeps),
            "ItemResult<System.Collections.Generic.ICollection<PersonResponse>>")]
        public void ReturnTypeNameForApi_UsesDtoForCollection(
            ClassViewModelData data, string methodName, string expectedReturn)
        {
            var method = data.ClassViewModel.MethodByName(methodName);
            Assert.Equal(expectedReturn, method.ApiActionReturnTypeDeclaration);
        }

        [Theory]
        [ClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.MethodWithOptionalCancellationToken), "cancellationToken",
            "default")]
        [ClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.MethodWithOptionalEnumParam), "status",
            "IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case.Statuses.Open")]
        public void OptionalParameter_HasCorrectDefaultValue(
            ClassViewModelData data, string methodName, string paramName, string expected)
        {
            var method = data.ClassViewModel.MethodByName(methodName);
            var param = method.Parameters.Single(p => p.Name == paramName);
            Assert.True(param.HasDefaultValue);
            Assert.Equal(expected, param.CsDefaultValue);
        }

        [Theory]
        [ClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.DownloadAttachment_VaryString), null)]
        [ClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.DownloadAttachment_CustomCache), 3600)]
        public void ClientCacheDuration_ReturnsCorrectValue(
            ClassViewModelData data, string methodName, int? expectedSeconds)
        {
            var method = data.ClassViewModel.MethodByName(methodName);
            var duration = method.ClientCacheDuration;
            
            if (expectedSeconds.HasValue)
            {
                Assert.NotNull(duration);
                Assert.Equal(TimeSpan.FromSeconds(expectedSeconds.Value), duration);
            }
            else
            {
                Assert.Null(duration);
            }
        }
    }
}

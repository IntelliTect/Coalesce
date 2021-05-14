using IntelliTect.Coalesce.Tests.Util;
using System;
using System.Collections.Generic;
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
            "ItemResult<System.Collections.Generic.ICollection<PersonDtoGen>>")]
        public void ReturnTypeNameForApi_UsesDtoForCollection(
            ClassViewModelData data, string methodName, string expectedReturn)
        {
            var method = data.ClassViewModel.MethodByName(methodName);
            Assert.Equal(expectedReturn, method.ApiActionReturnTypeDeclaration);
        }
    }
}

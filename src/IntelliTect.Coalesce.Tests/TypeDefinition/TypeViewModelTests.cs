using System;
using System.Collections.Generic;
using IntelliTect.Coalesce.TypeDefinition;
using Test.FooTypez.Data.Models;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class TypeViewModelTests
    {
        [Theory]
        [InlineData(typeof(FooType), "Test.FooTypez.Web.Models", "Test.FooTypez.Web.Models.FooTypeDtoGen")]
        [InlineData(typeof(BarType), "Test.FooTypez.Web.Models", "Test.FooTypez.Web.Models.BarTypeDtoGen")]
        [InlineData(typeof(ICollection<BarType>), "Test.FooTypez.Web.Models",
            "System.Collections.Generic.ICollection<Test.FooTypez.Web.Models.BarTypeDtoGen>")]
        [InlineData(typeof(ICollection<FooType>), "Test.FooTypez.Web.Models",
            "System.Collections.Generic.ICollection<Test.FooTypez.Web.Models.FooTypeDtoGen>")]
        public void WhenProjectNameSpaceContainsModelName_ItShouldNotReplaceNamespace(Type type, string @namespace,
            string expected)
        {
            var sut = new ReflectionTypeViewModel(type);
            var result = sut.NullableTypeForDto(@namespace);

            Assert.Equal(expected, result);
        }
    }
}

namespace Test.FooTypez.Data.Models
{
    public class FooType { }

    public class BarType { }
}
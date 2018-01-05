using System;
using System.Collections.Generic;
using IntelliTect.Coalesce.TypeDefinition;
using Test.BazTypez.Data.Models;
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
        [InlineData(typeof(FooType.FooTypez),"Test.FooTypez.Web.Models", "Test.FooTypez.Web.Models.FooType+FooTypezDtoGen" )]
        [InlineData(typeof(FooType[]),"Test.FooTypez.Web.Models", "Test.FooTypez.Web.Models.FooTypeDtoGen[]" )]
        [InlineData(typeof(ICollection<BazType<string>>),"Test.BazTypez.Web.Models", "System.Collections.Generic.ICollection<Test.BazTypez.Web.Models.BazTypeDtoGen<System.String>>" )]
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
    public class FooType
    {
        public class FooTypez
        {

        }
    }

    public class BarType { }
}

namespace Test.BazTypez.Data.Models
{
    public class BazType<T>
    {

    }
}
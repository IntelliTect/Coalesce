using System.Collections.Generic;
using IntelliTect.Coalesce.TypeDefinition;
using Moq;
using Moq.Protected;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class TypeViewModelTests
    {
        [Theory]
        [InlineData("Test.FooTypez.Data.Models", "FooType", "Test.FooTypez.Web.Models.FooTypeDtoGen" )]
        [InlineData("Test.FooTypez.Data.Models", "FooTypeDtoGen", "Test.FooTypez.Web.Models.FooTypeDtoGenDtoGen" )]
        [InlineData("Test.FooTypez.Data.Models", "BarType", "Test.FooTypez.Web.Models.BarTypeDtoGen" )]
        public void WhenProjectNameSpaceContainsModelName_ItShouldNotReplaceNamespace(string @namespace, string type, string expected)
        {
            var typeViewModel = new Mock<TypeViewModel>();
            typeViewModel.Setup(t => t.FullNamespace).Returns(@namespace);

            var classViewModel = new MockClassViewModel(typeViewModel.Object, type);


            var mock = new Mock<TypeViewModel>();

            mock.Setup(x => x.PureType.ClassViewModel).Returns(classViewModel);
            mock.Setup(x => x.IsNullable).Returns(true);
            mock.Setup(x => x.FullyQualifiedName).Returns($"{@namespace}.{type}");

            var sut = mock.Object;

            var result = sut.NullableTypeForDto("Test.FooTypez.Web.Models");

            Assert.Equal(expected, result);

        }

        public class MockClassViewModel : ClassViewModel
        {

            public MockClassViewModel(TypeViewModel typeViewModel, string name)
            {
                Type = typeViewModel;
                Name = name;
            }

            public override string Name { get; }
            public override string Comment { get; }
            protected override IReadOnlyCollection<PropertyViewModel> RawProperties { get; }
            protected override IReadOnlyCollection<MethodViewModel> RawMethods { get; }
            protected override IReadOnlyCollection<TypeViewModel> RawNestedTypes { get; }
        }
    }
}
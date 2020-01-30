using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Util
{
    public class ClassViewModelDataTest : IClassFixture<ClassViewModelDataCountTestFixture>
    {
        private readonly ClassViewModelDataCountTestFixture fixture;

        public ClassViewModelDataTest(ClassViewModelDataCountTestFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// I wrote this test when I discovered that ClassViewModelData had actually
        /// been providing ReflectionClassViewModel instances for both Symbol and Reflection versions of the test.
        /// </summary>
        /// <param name="data"></param>
        [Theory, ClassViewModelData(typeof(Bools))]
        public void ClassViewModelDataAttribute_EnsureProvidesBothTypesOfModels(ClassViewModelData data)
        {
            var type = data.ClassViewModel.GetType();

            fixture.TypeCounts[type] = fixture.TypeCounts.GetValueOrDefault(type) + 1;
        }
    }

    public class ClassViewModelDataCountTestFixture : IDisposable
    {
        public Dictionary<Type, int> TypeCounts { get; } = new Dictionary<Type, int>();

        public void Dispose()
        {
            var total = TypeCounts.Values.Sum();
            Assert.Equal(2, total);
            Assert.Equal(1, TypeCounts[typeof(ReflectionClassViewModel)]);
            Assert.Equal(1, TypeCounts[typeof(SymbolClassViewModel)]);
        }
    }
}

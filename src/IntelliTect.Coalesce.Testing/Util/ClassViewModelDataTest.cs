using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Testing.Util;


// Not compatible with the way that tunit runs tests, sometimes this works, sometimes it doesn't.

// [ClassDataSource<ClassViewModelDataCountTestFixture>(Shared = SharedType.PerAssembly)]
// public class ClassViewModelDataTest
// {
//     private readonly ClassViewModelDataCountTestFixture fixture;

//     public ClassViewModelDataTest(ClassViewModelDataCountTestFixture fixture)
//     {
//         this.fixture = fixture;
//     }

//     /// <summary>
//     /// I wrote this test when I discovered that ClassViewModelData had actually
//     /// been providing ReflectionClassViewModel instances for both Symbol and Reflection versions of the test.
//     /// </summary>
//     /// <param name="data"></param>
//     [Test, ClassViewModelData(typeof(Bools))]
//     public void ClassViewModelDataAttribute_EnsureProvidesBothTypesOfModels(ClassViewModelData data)
//     {
//         var type = data.ClassViewModel.GetType();

//         fixture.TypeCounts[type] = fixture.TypeCounts.GetValueOrDefault(type) + 1;
//     }
// }

// public class ClassViewModelDataCountTestFixture : IAsyncDisposable
// {
//     public Dictionary<Type, int> TypeCounts { get; } = new Dictionary<Type, int>();

//     public async ValueTask DisposeAsync()
//     {
//         var total = TypeCounts.Values.Sum();
//         await Assert.That(total).IsEqualTo(2);
//         await Assert.That(TypeCounts[typeof(ReflectionClassViewModel)]).IsEqualTo(1);
//         await Assert.That(TypeCounts[typeof(SymbolClassViewModel)]).IsEqualTo(1);
//     }
// }

using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class MethodViewModelTests
{
    [Test]
    [ClassViewModelData(
        typeof(Testing.TargetClasses.Github31.Person),
        nameof(Testing.TargetClasses.Github31.Person.GetMyPeeps),
        "ItemResult<System.Collections.Generic.ICollection<PersonResponse>>")]
    public async Task ReturnTypeNameForApi_UsesDtoForCollection(
        ClassViewModelData data, string methodName, string expectedReturn)
    {
        var method = data.ClassViewModel.MethodByName(methodName);
        await Assert.That(method.ApiActionReturnTypeDeclaration).IsEqualTo(expectedReturn);
    }

    [Test]
    [ClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.MethodWithOptionalCancellationToken), "cancellationToken",
        "default")]
    [ClassViewModelData(typeof(ComplexModel), nameof(ComplexModel.MethodWithOptionalEnumParam), "status",
        "IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext.Case.Statuses.Open")]
    public async Task OptionalParameter_HasCorrectDefaultValue(
        ClassViewModelData data, string methodName, string paramName, string expected)
    {
        var method = data.ClassViewModel.MethodByName(methodName);
        var param = method.Parameters.Single(p => p.Name == paramName);
        await Assert.That(param.HasDefaultValue).IsTrue();
        await Assert.That(param.CsDefaultValue).IsEqualTo(expected);
    }
}

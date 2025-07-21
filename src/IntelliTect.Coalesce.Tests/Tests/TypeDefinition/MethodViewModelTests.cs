using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System.Linq;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

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
}

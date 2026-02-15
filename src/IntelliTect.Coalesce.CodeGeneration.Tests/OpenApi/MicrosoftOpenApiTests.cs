#if NET9_0_OR_GREATER

namespace IntelliTect.Coalesce.CodeGeneration.Tests.OpenApi;

[ClassDataSource<OpenApiFixture>(Shared = SharedType.Keyed, Key = OpenApiFixture.Collection)]
public class MicrosoftOpenApiTests(OpenApiFixture fixture)
{
    public OpenApiFixture Fixture { get; } = fixture;

    // TODO: Still hopelessly broken until at least aspnetcore 9.0.4. Try again when its out.

    //[Fact]
    //public async Task Schema_DoesNotContainCrudStrategyClasses()
    //{
    //    var doc = await Fixture.GetDocumentAsync();

    //    Assert.DoesNotContain(doc.Components.Schemas, s =>
    //        s.Key.Contains("IDataSource", StringComparison.InvariantCultureIgnoreCase) ||
    //        s.Key.Contains("IBehaviors", StringComparison.InvariantCultureIgnoreCase)
    //    );
    //}

    //[Fact]
    //public async Task Schema_DoesNotContainInvalidDtoMembersInRequestBodies()
    //{
    //    var doc = await Fixture.GetDocumentAsync();

    //    var caseSaveProperties = doc
    //        .Paths["/api/Case/save"]
    //        .Operations[Microsoft.OpenApi.Models.OperationType.Post]
    //        .RequestBody.Content["multipart/form-data"]
    //        .Schema.Properties;

    //    // These members should not be in the parameters for endpoints that accept a DTO
    //    // because they cannot be mapped by the incoming DTO back to the entity.
    //    Assert.DoesNotContain(caseSaveProperties, p => p.Key.Contains("AssignedTo.", StringComparison.InvariantCultureIgnoreCase));
    //    Assert.DoesNotContain(caseSaveProperties, p => p.Key.Contains("CaseProducts", StringComparison.InvariantCultureIgnoreCase));
    //}

    //[Fact]
    //public async Task EndpointsWithObjectParameters_IncludeActionParameterNamePrefix()
    //{
    //    var doc = await Fixture.GetDocumentAsync();

    //    // We now generate [FromForm(Name = parameterName)] for all custom method parameters
    //    // in order to avoid situations where any of these are true:
    //    // 1) An endpoint has multiple parameters of the same type,
    //    // 2) An endpoint has a top level primitive parameter that has the same name
    //    //    as one of the properties on one of the method's object parameters.

    //    // In either of these scenarios, the aspnetcore modelbinder's behavior is not
    //    // strongly consistent without `Name` being set, and will vary based on the user input.
    //    // The same form parameter in the HTTP request may get bound to multiple parameters/objects,
    //    // and/or it may be impossible to bind _anything_ to one or more parameters.

    //    // However, setting Name has issues because it isn't well repsected by ApiExplorer/Swasbuckle.
    //    // This test is a verification of either:
    //    // 1) That https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2495 and/or
    //    //    https://github.com/dotnet/aspnetcore/issues/43815 have been fixed upstream, OR
    //    // 2) That CoalesceApiOperationFilter.PrefixParametersWithActionParameterName is working
    //    //    (although it isn't a perfect workaround because some of the scenarios
    //    //    that cause this issue lead to exceptions thrown by Swashbuckle before our filters run).

    //    var properties = doc
    //        .Paths["/api/ComplexModel/HasTopLevelParamWithSameNameAsObjectProp"]
    //        .Operations[Microsoft.OpenApi.Models.OperationType.Post]
    //        .RequestBody.Content["multipart/form-data"]
    //        .Schema.Properties;

    //    Assert.Single(properties, p => p.Key == "complexModelId");
    //    Assert.Single(properties, p => p.Key.Equals("model.complexModelId", StringComparison.InvariantCultureIgnoreCase));
    //}

    //[Fact]
    //public async Task DataSourceWithInheritance_DoesNotDuplicateSharedParameters()
    //{
    //    var doc = await Fixture.GetDocumentAsync();

    //    var parameters = doc
    //        .Paths["/api/Person/list"]
    //        .Operations[Microsoft.OpenApi.Models.OperationType.Get]
    //        .Parameters;

    //    var param = Assert.Single(parameters, p => p.Name == "dataSource.IntArray");
    //    Assert.Equal("Used by data sources ParameterTestsSource, ParameterTestsSourceSubclass.", param.Description);
    //}
}
#endif

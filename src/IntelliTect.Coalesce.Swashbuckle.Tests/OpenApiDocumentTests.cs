#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
using System.Threading.Tasks;
#else
using Microsoft.OpenApi.Models;
#endif

namespace IntelliTect.Coalesce.Swashbuckle.Tests;

[ClassDataSource<OpenApiFixture>(Shared = SharedType.Keyed, Key = OpenApiFixture.Collection)]
public class OpenApiDocumentTests
{
    public OpenApiDocumentTests(OpenApiFixture fixture)
    {
        Fixture = fixture;
    }

    public OpenApiFixture Fixture { get; }

    [Test]
    public async Task Schema_DoesNotContainCrudStrategyClasses()
    {
        var doc = await Fixture.GetDocumentAsync();

        await Assert.That(doc.Components.Schemas).DoesNotContain(s =>
            s.Key.Contains("IDataSource", StringComparison.InvariantCultureIgnoreCase) ||
            s.Key.Contains("IBehaviors", StringComparison.InvariantCultureIgnoreCase));
    }

    [Test]
    public async Task Schema_DoesNotContainInvalidDtoMembersInRequestBodies()
    {
        var doc = await Fixture.GetDocumentAsync();

        var caseSaveProperties = doc
            .Paths["/api/Case/save"]
#if NET10_0_OR_GREATER
            .Operations[HttpMethod.Post]
#else
            .Operations[OperationType.Post]
#endif
            .RequestBody.Content["multipart/form-data"]
            .Schema.Properties;

        // These members should not be in the parameters for endpoints that accept a DTO
        // because they cannot be mapped by the incoming DTO back to the entity.
        await Assert.That(caseSaveProperties).DoesNotContain(p => p.Key.Contains("AssignedTo.", StringComparison.InvariantCultureIgnoreCase));
        await Assert.That(caseSaveProperties).DoesNotContain(p => p.Key.Contains("CaseProducts", StringComparison.InvariantCultureIgnoreCase));
    }

    [Test]
    public async Task EndpointsWithObjectParameters_IncludeActionParameterNamePrefix()
    {
        var doc = await Fixture.GetDocumentAsync();

        // We now generate [FromForm(Name = parameterName)] for all custom method parameters
        // in order to avoid situations where any of these are true:
        // 1) An endpoint has multiple parameters of the same type,
        // 2) An endpoint has a top level primitive parameter that has the same name
        //    as one of the properties on one of the method's object parameters.

        // In either of these scenarios, the aspnetcore modelbinder's behavior is not
        // strongly consistent without `Name` being set, and will vary based on the user input.
        // The same form parameter in the HTTP request may get bound to multiple parameters/objects,
        // and/or it may be impossible to bind _anything_ to one or more parameters.

        // However, setting Name has issues because it isn't well repsected by ApiExplorer/Swasbuckle.
        // This test is a verification of either:
        // 1) That https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2495 and/or
        //    https://github.com/dotnet/aspnetcore/issues/43815 have been fixed upstream, OR
        // 2) That CoalesceApiOperationFilter.PrefixParametersWithActionParameterName is working
        //    (although it isn't a perfect workaround because some of the scenarios
        //    that cause this issue lead to exceptions thrown by Swashbuckle before our filters run).

        var properties = doc
            .Paths["/api/ComplexModel/HasTopLevelParamWithSameNameAsObjectProp"]
#if NET10_0_OR_GREATER
            .Operations[HttpMethod.Post]
#else
            .Operations[OperationType.Post]
#endif
            .RequestBody.Content["multipart/form-data"]
            .Schema.Properties;

        await Assert.That(properties).HasSingleItem();
        await Assert.That(properties).HasSingleItem();
    }

    [Test]
    public async Task DataSourceWithInheritance_DoesNotDuplicateSharedParameters()
    {
        var doc = await Fixture.GetDocumentAsync();

        var parameters = doc
            .Paths["/api/Person/list"]
#if NET10_0_OR_GREATER
            .Operations[HttpMethod.Get]
#else
            .Operations[OperationType.Get]
#endif
            .Parameters;

        var param = await Assert.That(parameters).HasSingleItem();
        await Assert.That(param.Description).IsEqualTo("Used by data sources ParameterTestsSource, ParameterTestsSourceSubclass.");
    }
}
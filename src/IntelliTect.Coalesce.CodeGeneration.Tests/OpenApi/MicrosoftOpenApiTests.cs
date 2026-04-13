#if NET9_0_OR_GREATER

#if NET10_0_OR_GREATER
using Microsoft.AspNetCore.TestHost;
using Microsoft.OpenApi;
using System.Net;
using System.Text.Json;
#endif

namespace IntelliTect.Coalesce.CodeGeneration.Tests.OpenApi;

[ClassDataSource<OpenApiFixture>(Shared = SharedType.Keyed, Key = OpenApiFixture.Collection)]
public class MicrosoftOpenApiTests(OpenApiFixture fixture)
{
    public OpenApiFixture Fixture { get; } = fixture;

#if NET10_0_OR_GREATER
    /// <summary>
    /// Workaround for https://github.com/dotnet/aspnetcore/issues/61038.
    /// Verifies that CoalesceNumericSchemaTransformer correctly fixes
    /// numeric types that otherwise get represented as string types with patterns.
    /// </summary>
    [Test]
    public async Task IntegerProperties_DoNotHaveStringType()
    {
        var client = Fixture.App.GetTestClient();
        var response = await client.GetAsync("/openapi/v1.json");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var schemas = doc.RootElement
            .GetProperty("components")
            .GetProperty("schemas");

        var issues = new List<string>();
        foreach (var schema in schemas.EnumerateObject())
        {
            if (!schema.Value.TryGetProperty("properties", out var properties)) continue;

            foreach (var prop in properties.EnumerateObject())
            {
                if (!prop.Value.TryGetProperty("format", out var format)) continue;
                var formatStr = format.GetString();

                if (formatStr is "int32" or "int64")
                {
                    if (!prop.Value.TryGetProperty("type", out var type))
                    {
                        issues.Add($"{schema.Name}.{prop.Name}: has format '{formatStr}' but no type");
                        continue;
                    }

                    // type could be a string ("integer") or an array (["integer", "string"])
                    if (type.ValueKind == JsonValueKind.String)
                    {
                        if (type.GetString() != "integer")
                        {
                            issues.Add($"{schema.Name}.{prop.Name}: expected type 'integer' but got '{type.GetString()}'");
                        }
                    }
                    else if (type.ValueKind == JsonValueKind.Array)
                    {
                        var types = type.EnumerateArray().Select(t => t.GetString()).ToList();
                        if (!types.Contains("integer"))
                        {
                            issues.Add($"{schema.Name}.{prop.Name}: type array [{string.Join(", ", types)}] does not contain 'integer'");
                        }
                        if (types.Contains("string"))
                        {
                            issues.Add($"{schema.Name}.{prop.Name}: type array should not contain 'string'");
                        }
                    }

                    if (prop.Value.TryGetProperty("pattern", out _))
                    {
                        issues.Add($"{schema.Name}.{prop.Name}: should not have pattern");
                    }
                }
            }
        }

        // Ensure we actually checked some properties (sanity check)
        await Assert.That(schemas.EnumerateObject().Any()).IsTrue();
        await Assert.That(issues).IsEmpty();
    }

    [Test]
    public async Task Schema_DoesNotContainCrudStrategyClasses()
    {
        var doc = await Fixture.GetDocumentAsync();

        await Assert.That(doc.Components.Schemas).DoesNotContain(s =>
            s.Key.Contains("IDataSource", StringComparison.InvariantCultureIgnoreCase) ||
            s.Key.Contains("IBehaviors", StringComparison.InvariantCultureIgnoreCase)
        );
    }

    [Test]
    public async Task Schema_DoesNotContainInvalidDtoMembersInRequestBodies()
    {
        var doc = await Fixture.GetDocumentAsync();

        var caseSaveProperties = doc
            .Paths["/api/Case/save"]
            .Operations[HttpMethod.Post]
            .RequestBody.Content["application/x-www-form-urlencoded"]
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

        // However, setting Name has issues because it isn't well respected by ApiExplorer.
        // This test is a verification of either:
        // 1) That https://github.com/dotnet/aspnetcore/issues/43815 has been fixed upstream, OR
        // 2) That CoalesceApiOperationFilter is working correctly.

        var schema = doc
            .Paths["/api/ComplexModel/HasTopLevelParamWithSameNameAsObjectProp"]
            .Operations[HttpMethod.Post]
            .RequestBody.Content["application/x-www-form-urlencoded"]
            .Schema;

        // In .NET 10, form schemas use allOf composition instead of inline properties.
        var properties = schema.Properties
            ?? schema.AllOf?.SelectMany(s => s.Properties ?? new Dictionary<string, IOpenApiSchema>())
                .ToDictionary(p => p.Key, p => p.Value)
            ?? new Dictionary<string, IOpenApiSchema>();

        await Assert.That(properties.Where(p => p.Key == "complexModelId")).HasSingleItem();
        await Assert.That(properties.Where(p => p.Key.Equals("model.complexModelId", StringComparison.InvariantCultureIgnoreCase))).HasSingleItem();
    }

    [Test]
    public async Task DataSourceWithInheritance_DoesNotDuplicateSharedParameters()
    {
        var doc = await Fixture.GetDocumentAsync();

        var parameters = doc
            .Paths["/api/Person/list"]
            .Operations[HttpMethod.Get]
            .Parameters;

        var param = await Assert.That(parameters.Where(p => p.Name == "dataSource.IntArray")).HasSingleItem();
        await Assert.That(param.Description).IsEqualTo("Used by data sources ParameterTestsSource, ParameterTestsSourceSubclass.");
    }
#endif
}
#endif

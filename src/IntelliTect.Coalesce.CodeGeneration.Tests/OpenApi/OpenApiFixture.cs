
#if NET9_0_OR_GREATER

using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Microsoft.OpenApi.Readers;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Tests.OpenApi;

[System.Obsolete]
public class OpenApiFixtureCollection
{ }

public class OpenApiFixture
{
    public const string Collection = "OpenApi";

    public OpenApiFixture()
    {
        Assembly assembly = CodeGenTestBase.WebAssembly.Value;

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddCoalesce<AppDbContext>();
                    services.AddMvc()
                        // Add our dynamic assembly that contains our generated controllers and DTOs
                        .AddApplicationPart(assembly);
                    services.AddOpenApi();
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(b =>
                    {
                        b.MapOpenApi();
                    });
                });
            });

        App = hostBuilder.Start();
    }

    public IHost App { get; }

    public async Task<OpenApiDocument> GetDocumentAsync()
    {
        var client = App.GetTestClient();

        var openApiDoc = await client.GetAsync("/openapi/v1.json");
        await Assert.That(openApiDoc.StatusCode).IsEqualTo(HttpStatusCode.OK);

        // OpenApiDocument cannot be parsed directly with a JSON deserializer,
        // as it is a midly non-normalized format that requires special rules to understand.
#if NET10_0_OR_GREATER
        var result = await OpenApiDocument.LoadAsync(await openApiDoc.Content.ReadAsStreamAsync(), "json");
        await Assert.That(result.Document).IsNotNull();
        await Assert.That(result.Diagnostic.Errors).IsEmpty();
        await Assert.That(result.Diagnostic.Warnings).IsEmpty();
        return result.Document;
#else
        var openApiDocument = new OpenApiStreamReader()
            .Read(await openApiDoc.Content.ReadAsStreamAsync(), out var diagnostic);
        Assert.NotNull(openApiDocument);
        Assert.Empty(diagnostic.Errors);
        Assert.Empty(diagnostic.Warnings);
        return openApiDocument;
#endif
    }
}#endif
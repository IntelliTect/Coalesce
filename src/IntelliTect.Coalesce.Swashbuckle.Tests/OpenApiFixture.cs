using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Net;
using System.Reflection;
using Xunit;

namespace IntelliTect.Coalesce.Swashbuckle.Tests
{
    [CollectionDefinition(OpenApiFixture.Collection)]
    public class OpenApiFixtureCollection : ICollectionFixture<OpenApiFixture> { }

    public class OpenApiFixture
    {
        public const string Collection = "OpenApi";

        public OpenApiFixture()
        {
            var generationTasks = new GenerationExecutor(
                    new() { WebProject = new() { RootNamespace = "MyProject" } },
                    Microsoft.Extensions.Logging.LogLevel.Information
                )
                .CreateRootGenerator<ApiOnlySuite>()
                .WithModel(ReflectionRepositoryFactory.Symbol)
                .WithOutputPath(".")
                .GetGeneratorsFlattened()
                .OfType<IFileGenerator>()
                .Select(gen => (Generator: gen, Output: gen.GetOutputAsync()));

            Task.WaitAll(generationTasks.Select(t => t.Output).ToArray());

            var generatedFiles = generationTasks
                .Select((task) => CSharpSyntaxTree.ParseText(
                    SourceText.From(new StreamReader(task.Output.Result).ReadToEnd()),
                    path: task.Generator.EffectiveOutputPath
                ))
                .ToArray();

            using var ms = new MemoryStream();
            EmitResult emitResult = ReflectionRepositoryFactory
                .GetCompilation(generatedFiles)
                .Emit(ms);
            Assert.True(emitResult.Success);
            Assembly assembly = Assembly.Load(ms.ToArray());

            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureServices(services =>
                    {
                        services.AddCoalesce<TestDbContext>();
                        services.AddMvc()
                            // Add our dynamic assembly that contains our generated controllers and DTOs
                            .AddApplicationPart(assembly);
                        services.AddSwaggerGen(c =>
                        {
                            c.AddCoalesce();
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                        });
                    });
                    webHost.Configure(app =>
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI(c =>
                        {
                            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                        });
                    });
                });

            App = hostBuilder.Start();
        }

        public IHost App { get; }

        public async Task<OpenApiDocument> GetDocumentAsync()
        {
            var client = App.GetTestClient();

            var openApiDoc = await client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, openApiDoc.StatusCode);

            // OpenApiDocument cannot be parsed directly with a JSON deserializer,
            // as it is a midly non-normalized format that requires special rules to understand.
            var openApiDocument = new OpenApiStreamReader()
                .Read(await openApiDoc.Content.ReadAsStreamAsync(), out var diagnostic);
            Assert.NotNull(openApiDocument);
            Assert.Empty(diagnostic.Errors);
            Assert.Empty(diagnostic.Warnings);
            return openApiDocument;
        }

        public class ApiOnlySuite : CompositeGenerator<ReflectionRepository>, IRootGenerator
        {
            public ApiOnlySuite(CompositeGeneratorServices services) : base(services) { }

            public override IEnumerable<IGenerator> GetGenerators()
            {
                yield return Generator<IntelliTect.Coalesce.CodeGeneration.Api.Generators.Models>()
                    .WithModel(Model)
                    .AppendOutputPath("Models");

                yield return Generator<Controllers>()
                    .WithModel(Model);
            }
        }
    }
}
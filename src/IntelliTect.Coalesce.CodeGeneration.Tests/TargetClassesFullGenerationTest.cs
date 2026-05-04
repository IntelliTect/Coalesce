using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Vue.Generators;
using IntelliTect.Coalesce.Testing;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using TUnit.Core.Interfaces;

namespace IntelliTect.Coalesce.CodeGeneration.Tests;

public class VueSuiteFixture : IAsyncInitializer
{
    public IRootGenerator Suite { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var executor = new GenerationExecutor(
            new CoalesceConfiguration
            {
                WebProject = new ProjectConfiguration { RootNamespace = "MyProject" }
            },
            Microsoft.Extensions.Logging.LogLevel.Information
        );

        var suite = executor.CreateRootGenerator<VueSuite>()
            .WithModel(ReflectionRepositoryFactory.Symbol);

        var validationResult = ValidateContext.Validate(suite.Model);
        await Assert.That(validationResult.Where(r => r.IsError)).IsEmpty();

        var tfmAttr = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>()!;
        var outDir = Path.Combine(CodeGenTestBase.GetRepoRoot().FullName, "src", "IntelliTect.Coalesce.CodeGeneration.Tests", "out", tfmAttr.FrameworkName!, "VueSuite");
        suite = suite.WithOutputPath(outDir);

        await suite.GenerateAsync();
        Suite = suite;
    }
}

[ClassDataSource<VueSuiteFixture>(Shared = SharedType.PerTestSession)]
public class TargetClassesFullGenerationTest(VueSuiteFixture fixture) : CodeGenTestBase
{
    // #IF directive so this doesn't needlessly run for multiple TFMs. It only needs to run for one.
#if NET10_0
    /// <summary>
    /// This test isn't asserting anything in .NET land.
    /// Its purpose is to produce generated files that we can build our 
    /// typescript/javascript tests upon, rather than building out
    /// metadata, models, api clients, and viewmodels by hand.
    /// </summary>
    [Test]
    public async Task CreateVitestTargets()
    {
        var executor = BuildExecutor();
        executor.Config.GeneratorConfig = JsonConvert.DeserializeObject<Dictionary<string, JObject>>("""
        {
            "Controllers": {
                "disabled": true
            },
            "Models": {
                "disabled": true
            },
            "KernelPlugins": {
                "disabled": true
            },
            "Scripts": {
                "targetDirectory": "../"
            }
        }
        """);

        var project = GetRepoRoot().GetDirectory("src/test-targets");

        var suite = executor.CreateRootGenerator<VueSuite>()
            .WithModel(ReflectionRepositoryFactory.Symbol)
            .WithOutputPath(project.FullName);

        await suite.GenerateAsync();
    }
#endif

    [Test]
    [Arguments("5")]
    [Arguments("5.9")]
    [Arguments("6")]
    public async Task VueOutputTypescriptCompiles(string tsVersion)
    {
        var outputPath = fixture.Suite.EffectiveOutputPath.Replace("\\", "/");

        var tsConfig =
        $$"""
        {
          "compilerOptions": {
            "target": "ES2022",
            "module": "preserve",
            "moduleResolution": "bundler",
            "strict": true,
            "verbatimModuleSyntax": true,
            "paths": {
              "coalesce-vue/lib/*": [ "../../node_modules/coalesce-vue/src/*" ],
            },
            "types": ["vue-router"]
          },
          "include": [
            "src/**/*.ts",
          ],
        }
        """;
        var tsConfigPath = $"{outputPath}/tsconfig.{tsVersion}.json";
        File.WriteAllText(tsConfigPath, tsConfig);

        await AssertTypescriptProjectCompiles(
            tsConfigPath: tsConfigPath,
            workingDirectory: outputPath,
            tsVersion: tsVersion
        );
    }

    [Test]
    public async Task VueOutputCSharpCompiles()
    {
        await AssertSuiteCSharpOutputCompiles(fixture.Suite);
    }

    [Test]
    public async Task SecurityOverviewDataGenerates()
    {
        // This test probably doesn't quite belong in this class

        var services = new ServiceCollection()
            .AddCoalesce(c =>
            {
                c.AddContext<AppDbContext>();
            })
            .AddSingleton<IWebHostEnvironment>(Mock.Of<IWebHostEnvironment>())
            .AddScoped<AppDbContext>() // good enough (doesn't need to be configured, just needs to exist)
            .BuildServiceProvider();

        var reflectionData = CoalesceApplicationBuilderExtensions.GetSecurityOverviewData(
            ReflectionRepositoryFactory.Reflection,
            new DataSourceFactory(services, ReflectionRepositoryFactory.Reflection),
            new BehaviorsFactory(services, ReflectionRepositoryFactory.Reflection)
        );

        // We serialize here to force evaluation of all the lazy IEnumerables in the data.
        // Otherwise, we wouldn't be testing very much here.
        // We're ultimately just testing that this doesn't throw right now.
        // Not looking for any specific output.
        await Assert.That(System.Text.Json.JsonSerializer.Serialize(reflectionData)).IsNotNull();
    }
}

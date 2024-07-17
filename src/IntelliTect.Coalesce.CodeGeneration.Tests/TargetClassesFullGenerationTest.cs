using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Knockout.Generators;
using IntelliTect.Coalesce.CodeGeneration.Vue.Generators;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    public class TargetClassesFullGenerationTest : CodeGenTestBase
    {
#if NET8_0
        /// <summary>
        /// This test isn't asserting anything in .NET land.
        /// Its purpose is to produce generated files that we can build our 
        /// typescript/javascript tests upon, rather than building out
        /// metadata, models, api clients, and viewmodels by hand.
        /// </summary>
        [Fact]
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

        [Fact]
        public async Task VueOutputCompiles()
        {
            var executor = BuildExecutor();

            var suite = executor.CreateRootGenerator<VueSuite>()
                .WithModel(ReflectionRepositoryFactory.Symbol);
            suite = await ConfigureAndValidateSuite(suite);
            await suite.GenerateAsync();

            await Task.WhenAll(
                Task.Run(() => AssertVueSuiteTypescriptOutputCompiles(suite, "5")),
                Task.Run(() => AssertVueSuiteTypescriptOutputCompiles(suite, "5.2")),
                Task.Run(() => AssertVueSuiteTypescriptOutputCompiles(suite, "5.5")),
                Task.Run(() => AssertSuiteCSharpOutputCompiles(suite))
            );
        }

        protected async Task AssertVueSuiteTypescriptOutputCompiles(IRootGenerator suite, string tsVersion)
        {
            var coalesceVue = GetRepoRoot().GetDirectory("src/coalesce-vue");

            Assert.True(
                coalesceVue.GetDirectory("node_modules").Exists,
                "Test relies on NPM packages for coalesce-vue being restored."
            );

            // We use coalesce-vue as our working directory here
            // because it contains both tsc and all the dependencies of the generated code.
            var workingDirectory = coalesceVue.FullName.Replace("\\", "/");
            var tsConfig =
            $$"""
            {
              "compilerOptions": {
                "target": "ES2021",
                "strict": true,
                "moduleResolution": "node",
                "baseUrl": ".",
                "paths": {
                  "coalesce-vue/lib/*": [ "{{workingDirectory}}/src/*" ],
                  "*": [ "{{workingDirectory}}/node_modules/*" ],
                },
                "types": ["{{workingDirectory}}/node_modules/vue-router"]
              },
              "include": [
                "src/**/*.ts",
              ],
            }
            """;
            var tsConfigPath = $"{suite.EffectiveOutputPath}/tsconfig.{tsVersion}.json";
            File.WriteAllText(tsConfigPath, tsConfig);

            await AssertTypescriptProjectCompiles(
                tsConfigPath: tsConfigPath,
                workingDirectory: workingDirectory,
                tsVersion: tsVersion
            );
        }

        [Fact]
        public async Task KnockoutOutputCompiles()
        {
            var executor = BuildExecutor();

            var suite = executor.CreateRootGenerator<KnockoutSuite>()
                .WithModel(ReflectionRepositoryFactory.Symbol);
            suite = await ConfigureAndValidateSuite(suite);
            await suite.GenerateAsync();

            await AssertSuiteCSharpOutputCompiles(suite);
        }

        [Fact]
        public void SecurityOverviewDataGenerates()
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

            // We serialize here to force evaulation of all the lazy IEnumerables in the data.
            // Otherwise, we wouldn't be testing very much here.
            // We're ultimately just testing that this doesn't throw right now.
            // Not looking for any specific output.
            Assert.NotNull(System.Text.Json.JsonSerializer.Serialize(reflectionData));
        }
    }
}

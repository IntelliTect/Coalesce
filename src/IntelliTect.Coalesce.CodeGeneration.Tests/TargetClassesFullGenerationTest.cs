using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.Util;
using System;
using System.Linq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.IO;
using IntelliTect.Coalesce.CodeGeneration.Vue.Generators;
using IntelliTect.Coalesce.CodeGeneration.Knockout.Generators;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using System.Text.Json;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    public class TargetClassesFullGenerationTest : CodeGenTestBase
    {
        [Fact]
        public async Task VueOutputCompiles()
        {
            var executor = BuildExecutor();

            var suite = executor.CreateRootGenerator<VueSuite>()
                .WithModel(ReflectionRepositoryFactory.Symbol);
            suite = await ConfigureAndValidateSuite(suite);

            await Task.WhenAll(
                Task.Run(() => AssertVueSuiteTypescriptOutputCompiles(suite)),
                Task.Run(() => AssertSuiteCSharpOutputCompiles(suite))
            );
        }

        protected async Task AssertVueSuiteTypescriptOutputCompiles(IRootGenerator suite)
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
                "target": "ES2020",
                "strict": true,
                "moduleResolution": "node",
                "baseUrl": ".",
                "paths": {
                  "coalesce-vue/lib/*": [ "{{workingDirectory}}/src/*" ],
                  "*": [ "{{workingDirectory}}/node_modules/*" ],
                }
              },
              "include": [
                "src/**/*.ts",
              ],
            }
            """;
            var tsConfigPath = suite.EffectiveOutputPath + "/tsconfig.json";
            File.WriteAllText(tsConfigPath, tsConfig);

            await AssertTypescriptProjectCompiles(
                tsConfigPath: tsConfigPath, 
                workingDirectory: workingDirectory);
        }

        [Fact]
        public async Task KnockoutOutputCompiles()
        {
            var executor = BuildExecutor();

            var suite = executor.CreateRootGenerator<KnockoutSuite>()
                .WithModel(ReflectionRepositoryFactory.Symbol);
            suite = await ConfigureAndValidateSuite(suite);

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
            Assert.NotNull(JsonSerializer.Serialize(reflectionData));
        }
    }
}

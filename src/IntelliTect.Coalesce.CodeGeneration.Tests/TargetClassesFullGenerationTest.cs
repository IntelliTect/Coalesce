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

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    public class TargetClassesFullGenerationTest : CodeGenTestBase
    {
        [Fact]
        public async Task VueOutputCompiles()
        {
            var executor = BuildExecutor();

            var suite = new VueSuite(executor.ServiceProvider.GetService<CompositeGeneratorServices>())
                .WithModel(ReflectionRepositoryFactory.Symbol);

            await AssertSuiteOutputCompiles(suite);
        }

        [Fact]
        public async Task KnockoutOutputCompiles()
        {
            var executor = BuildExecutor();

            var suite = new KnockoutSuite(executor.ServiceProvider.GetService<CompositeGeneratorServices>())
                .WithModel(ReflectionRepositoryFactory.Symbol);

            await AssertSuiteOutputCompiles(suite);
        }

        [Fact]
        public void SecurityOverviewDataGenerates()
        {
            // This test probably doesn't quite belong in this class

            var services = new ServiceCollection()
                .AddCoalesce(c =>
                {
                    c.AddContext<TestDbContext>();
                })
                .AddScoped<TestDbContext>() // good enough (doesn't need to be configured, just needs to exist)
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

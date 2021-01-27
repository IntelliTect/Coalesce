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
    }
}

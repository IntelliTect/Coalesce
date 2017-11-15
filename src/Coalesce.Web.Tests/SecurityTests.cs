using Coalesce.Web.Tests.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace Coalesce.Web.Tests
{
    public class SecurityTests : IDisposable
    {
        private Process _process;
        private ProjectContext _dataContext;
        private List<ClassViewModel> _models;
        private readonly Uri _apiUri = new Uri("http://localhost:5000");

        public SecurityTests()
        {
            _process = Processes.StartDotNet();
            _dataContext = new RoslynProjectContextFactory(null).CreateContext(new ProjectConfiguration
            {
                ProjectFile = @"..\..\..\..\..\Coalesce.Domain.csproj"
            });

            var typeLocator = _dataContext.TypeLocator;
            var contextSymbol = typeLocator.FindType("AppDbContext", throwWhenNotFound: false);
            _models = ReflectionRepository.Global
                            .AddContext(contextSymbol)
                            .Where(m => m.PrimaryKey != null)
                            .ToList();
        }

        [Fact(Skip = "Run Explicitly By Removing Skip", DisplayName = "Security Tests: Remove Skip To Run; Best To Run Alone")]
        //[Fact]
        public async Task AllTests()
        {
            using (var output = new StreamWriter("output.txt"))
            {
                var model = new TestModel
                {
                    Models = _models,
                    ApiUrl = _apiUri
                };

                // Check for any issues from an anonymous user
                var anonTests = new SecurityTestsAnonymous(model, output);
                var anonResults = await anonTests.RunTests();
                Assert.True(anonResults);

                // And admin users
                var adminTests = new SecurityTestsAdmin(model, output);
                var adminResults = await adminTests.RunTests();
                Assert.True(adminResults);

                // And regular users
                var userTests = new SecurityTestsUser(model, output);
                var userResults = await userTests.RunTests();
                Assert.True(userResults);
            }

            // Process.Start("output.txt");
        }

        public void Dispose()
        {
            try
            {
                _process.Kill();
            }
            catch { }
            try
            {
                _process.Dispose();
            }
            catch { }
        }
    }
}

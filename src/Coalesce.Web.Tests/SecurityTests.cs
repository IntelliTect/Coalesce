using Coalesce.Web.Tests.Helpers;
using IntelliTect.Coalesce.CodeGeneration;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Common;
using Xunit;

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
            _dataContext = MsBuildProjectContextBuilder.CreateContext(@"..\..\..\..\..\Coalesce.Domain");

            var typeLocator = IntelliTect.Coalesce.CodeGeneration.Scripts.ModelTypesLocator.FromProjectContext(_dataContext);
            var contextSymbol = ValidationUtil.ValidateType("AppDbContext", "dataContext", typeLocator, throwWhenNotFound: false);
            _models = ReflectionRepository
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
                Assert.Equal(true, anonResults);

                // And admin users
                var adminTests = new SecurityTestsAdmin(model, output);
                var adminResults = await adminTests.RunTests();
                Assert.Equal(true, adminResults);

                // And regular users
                var userTests = new SecurityTestsUser(model, output);
                var userResults = await userTests.RunTests();
                Assert.Equal(true, userResults);
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

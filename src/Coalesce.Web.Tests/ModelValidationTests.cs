using Coalesce.Domain;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Web.Tests
{
    public class ModelValidationTests
    {
        [Fact]
        public void ModelValidation()
        {
            var models = ReflectionRepository.Global.AddContext<AppDbContext>();
            var result = ValidateContext.Validate(models);

            foreach (var test in result)
            {
                Assert.True(test.WasSuccessful, $"{test.Area}: {test.Message}");
            }
        }
    }
}

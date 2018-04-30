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
            var rr = new ReflectionRepository();
            rr.AddAssembly<AppDbContext>();
            var result = ValidateContext.Validate(rr);

            foreach (var test in result)
            {
                Assert.True(test.WasSuccessful, $"{test.Area}: {test.Message}");
            }
        }
    }
}

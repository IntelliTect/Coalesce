using Coalesce.Domain;
using Intellitect.ComponentModel.Validation;
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
            var result = ValidateContext.Validate<DbContext>();

            foreach (var test in result)
            {
                Assert.True(test.WasSuccessful, $"{test.Area}: {test.Message}");
            }
        }
    }
}

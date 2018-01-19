using IntelliTect.Coalesce.Utilities;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Utilities
{
    public class StringExtensionTests
    {
        [Fact]
        public void CanGetIdentifierWithSpaces()
        {
            string input = "Oh No";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("Oh\\u2000No", identifier);
        }

        [Fact]
        public void CanGetIdentifierWithPrefix()
        {
            string input = "MyVariable";

            string identifier = input.GetValidCSharpIdentifier("_");

            Assert.Equal("_MyVariable", identifier);
        }

        [Fact]
        public void CanGetValidIdentifierWhenItIsAlreadyValid()
        {
            string input = "IamValidCSharp";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal(input, identifier);
        }
    }
}

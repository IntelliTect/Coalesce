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

            Assert.Equal("Oh_No", identifier);
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

            Assert.Equal("IamValidCSharp", identifier);
        }

        [Fact]
        public void CanGetValidIdentifierWhenSourceContainsUnderscore()
        {
            string input = "_in_put_";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("_in_put_", identifier);
        }

        [Fact]
        public void CanGetValidIdentifierWhenSourceContainsUnicode()
        {
            string input = @"Uni\u15F0code";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("Uni\\u15F0code", identifier);
        }

        [Fact]
        public void GetGetIdentifierForKeyword()
        {
            string input = "class";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("@class", identifier);
        }

        [Fact]
        public void CanGetIdentifierForNumericString()
        {
            string input = "0123";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("_0123", identifier);
        }

        [Fact]
        public void CanGetIndetifierFromVerbatimIdentifier()
        {
            string input = "@if";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("@if", identifier);
        }
    }
}

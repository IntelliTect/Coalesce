using IntelliTect.Coalesce.Utilities;
using System.Collections.Generic;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Utilities
{
    public class StringExtensionTests
    {
        [Fact]
        public void GetValidCSharpIdentifier_CanGetIdentifierWithSpaces()
        {
            string input = "Oh No";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("Oh_No", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_CanGetIdentifierWithPrefix()
        {
            string input = "MyVariable";

            string identifier = input.GetValidCSharpIdentifier("_");

            Assert.Equal("_MyVariable", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_CanGetValidIdentifierWhenItIsAlreadyValid()
        {
            string input = "IamValidCSharp";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("IamValidCSharp", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_CanGetValidIdentifierWhenSourceContainsUnderscore()
        {
            string input = "_in_put_";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("_in_put_", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_CanGetValidIdentifierWhenSourceContainsUnicode()
        {
            string input = @"Uni\u15F0code";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("Uni\\u15F0code", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_GetGetIdentifierForKeyword()
        {
            string input = "class";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("@class", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_CanGetIdentifierForNumericString()
        {
            string input = "0123";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("_0123", identifier);
        }

        [Fact]
        public void GetValidCSharpIdentifier_CanGetIndetifierFromVerbatimIdentifier()
        {
            string input = "@if";

            string identifier = input.GetValidCSharpIdentifier();

            Assert.Equal("@if", identifier);
        }

        [Theory]
        [InlineData("case")]
        [InlineData("await")]
        public void GetValidCSharpIdentifier_EscapesReservedWord(string input)
        {
            Assert.Equal("@" + input, input.GetValidCSharpIdentifier());
        }

        [Fact]
        public void ToProperCase_HandlesTwoCapitalsInARow()
        {
            const string input = "HROnly";
            Assert.Equal("HR Only", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesMultipleCapitalsInARow()
        {
            const string input = "FBIOnly";
            Assert.Equal("FBI Only", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesOnlyTwoCapitals()
        {
            const string input = "UI";
            Assert.Equal("UI", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesANormalWord()
        {
            const string input = "User";
            Assert.Equal("User", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesANormalPhrase()
        {
            const string input = "HelloWorld";
            Assert.Equal("Hello World", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesCamelCase()
        {
            const string input = "helloWorld";
            Assert.Equal("Hello World", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesCamelCaseWithAcronym()
        {
            const string input = "helloWorldUI";
            Assert.Equal("Hello World UI", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesNumberBeforeLowercase()
        {
            const string input = "Is1h";
            Assert.Equal("Is 1h", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesNumbersBeforeLowercase()
        {
            const string input = "Is24h";
            Assert.Equal("Is 24h", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesNumberBeforeUppercase()
        {
            const string input = "Is3D";
            Assert.Equal("Is 3D", input.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesNumbersBeforeUppercase()
        {
            const string input = "Is365Days";
            Assert.Equal("Is 365 Days", input.ToProperCase());
        }
    }
}

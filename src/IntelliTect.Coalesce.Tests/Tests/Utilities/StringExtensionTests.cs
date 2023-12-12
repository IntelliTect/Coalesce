using IntelliTect.Coalesce.Utilities;
using System.Collections.Generic;
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

        [Fact]
        public void ToProperCase_HandlesTwoCapitalsInARow()
        {
            const string enumName = "HROnly";
            Assert.Equal("HR Only", enumName.ToProperCase());
        }
        [Fact]
        public void ToProperCase_HandlesMultipleCapitalsInARow()
        {
            const string enumName = "FBIOnly";
            Assert.Equal("FBI Only", enumName.ToProperCase());
        }
        [Fact]
        public void ToProperCase_HandlesOnlyTwoCapitals()
        {
            const string enumName = "UI";
            Assert.Equal("UI", enumName.ToProperCase());
        }
        [Fact]
        public void ToProperCase_HandlesANormalWord()
        {
            const string enumName = "User";
            Assert.Equal("User", enumName.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesANormalPhrase()
        {
            const string enumName = "HelloWorld";
            Assert.Equal("Hello World", enumName.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesCamelCase()
        {
            const string enumName = "helloWorld";
            Assert.Equal("Hello World", enumName.ToProperCase());
        }

        [Fact]
        public void ToProperCase_HandlesCamelCaseWithAcronym()
        {
            const string enumName = "helloWorldUI";
            Assert.Equal("Hello World UI", enumName.ToProperCase());
        }
    }
}

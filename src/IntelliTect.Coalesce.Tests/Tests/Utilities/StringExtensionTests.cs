using IntelliTect.Coalesce.Utilities;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.Utilities;

public class StringExtensionTests
{
    [Test]
    public async Task GetValidCSharpIdentifier_CanGetIdentifierWithSpaces()
    {
        string input = "Oh No";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("Oh_No");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_CanGetIdentifierWithPrefix()
    {
        string input = "MyVariable";

        string identifier = input.GetValidCSharpIdentifier("_");

        await Assert.That(identifier).IsEqualTo("_MyVariable");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_CanGetValidIdentifierWhenItIsAlreadyValid()
    {
        string input = "IamValidCSharp";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("IamValidCSharp");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_CanGetValidIdentifierWhenSourceContainsUnderscore()
    {
        string input = "_in_put_";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("_in_put_");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_CanGetValidIdentifierWhenSourceContainsUnicode()
    {
        string input = @"Uni\u15F0code";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("Uni\\u15F0code");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_GetGetIdentifierForKeyword()
    {
        string input = "class";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("@class");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_CanGetIdentifierForNumericString()
    {
        string input = "0123";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("_0123");
    }

    [Test]
    public async Task GetValidCSharpIdentifier_CanGetIndetifierFromVerbatimIdentifier()
    {
        string input = "@if";

        string identifier = input.GetValidCSharpIdentifier();

        await Assert.That(identifier).IsEqualTo("@if");
    }

    [Test]
    [Arguments("case")]
    [Arguments("await")]
    public async Task GetValidCSharpIdentifier_EscapesReservedWord(string input)
    {
        await Assert.That(input.GetValidCSharpIdentifier()).IsEqualTo("@" + input);
    }

    [Test]
    public async Task ToProperCase_HandlesTwoCapitalsInARow()
    {
        const string input = "HROnly";
        await Assert.That(input.ToProperCase()).IsEqualTo("HR Only");
    }

    [Test]
    public async Task ToProperCase_HandlesMultipleCapitalsInARow()
    {
        const string input = "FBIOnly";
        await Assert.That(input.ToProperCase()).IsEqualTo("FBI Only");
    }

    [Test]
    public async Task ToProperCase_HandlesOnlyTwoCapitals()
    {
        const string input = "UI";
        await Assert.That(input.ToProperCase()).IsEqualTo("UI");
    }

    [Test]
    public async Task ToProperCase_HandlesANormalWord()
    {
        const string input = "User";
        await Assert.That(input.ToProperCase()).IsEqualTo("User");
    }

    [Test]
    public async Task ToProperCase_HandlesANormalPhrase()
    {
        const string input = "HelloWorld";
        await Assert.That(input.ToProperCase()).IsEqualTo("Hello World");
    }

    [Test]
    public async Task ToProperCase_HandlesCamelCase()
    {
        const string input = "helloWorld";
        await Assert.That(input.ToProperCase()).IsEqualTo("Hello World");
    }

    [Test]
    public async Task ToProperCase_HandlesCamelCaseWithAcronym()
    {
        const string input = "helloWorldUI";
        await Assert.That(input.ToProperCase()).IsEqualTo("Hello World UI");
    }

    [Test]
    public async Task ToProperCase_HandlesNumberBeforeLowercase()
    {
        const string input = "Is1h";
        await Assert.That(input.ToProperCase()).IsEqualTo("Is 1h");
    }

    [Test]
    public async Task ToProperCase_HandlesNumbersBeforeLowercase()
    {
        const string input = "Is24h";
        await Assert.That(input.ToProperCase()).IsEqualTo("Is 24h");
    }

    [Test]
    public async Task ToProperCase_HandlesNumberBeforeUppercase()
    {
        const string input = "Is3D";
        await Assert.That(input.ToProperCase()).IsEqualTo("Is 3D");
    }

    [Test]
    public async Task ToProperCase_HandlesNumbersBeforeUppercase()
    {
        const string input = "Is365Days";
        await Assert.That(input.ToProperCase()).IsEqualTo("Is 365 Days");
    }
}

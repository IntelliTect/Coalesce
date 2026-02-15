namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce1001_SimplifyItemResultTests : CSharpAnalyzerVerifier<Coalesce1001_SimplifyItemResult>
{
    public Coalesce1001_SimplifyItemResultTests()
    {
        DisabledDiagnostics = ["COA1002"];
    }

    [Test]
    public async Task ItemResult_BooleanConstructor_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult(true)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_BooleanConstructorTargetTypedNew_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new(true)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_PropertyInitializer_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult { WasSuccessful = true }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_BooleanConstructor_False_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult(false)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return false;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_PropertyInitializer_False_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult { WasSuccessful = false }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return false;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_StringConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult("Error message")|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return "Error message";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_PropertyInitializer_ErrorMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult { WasSuccessful = false, Message = $"Error message: {42}" }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return $"Error message: {42}";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_StringConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COA1001:new ItemResult<int>("Error message")|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return "Error message";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_PropertyInitializer_ErrorMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COA1001:new ItemResult<int> { WasSuccessful = false, Message = "Error message" }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return "Error message";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_ObjectConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COA1001:new ItemResult<int>(42)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return 42;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_PropertyInitializer_Object_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COA1001:new ItemResult<int> { Object = 42 }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return 42;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_ObjectVariable_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    var value = 42;
                    return {|COA1001:new ItemResult<int>(value)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    var value = 42;
                    return value;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_PropertyInitializer_ObjectVariable_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    var value = 42;
                    return {|COA1001:new ItemResult<int> { Object = value }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    var value = 42;
                    return value;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_MultiArgumentWithObject_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COA1001:new ItemResult<int>(true, null, 42)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return 42;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_PropertyInitializer_SuccessWithObject_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COA1001:new ItemResult<int> { WasSuccessful = true, Object = 42 }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return 42;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultString_StringConstructor_NoWarning()
    {
        // ItemResult<string> with string constructor is ambiguous, should not report
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return new ItemResult<string>(errorMessage: "Error message");
                }

                public ItemResult<string> GetResult2()
                {
                    return new ItemResult<string>(obj: "Error message");
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_BooleanWithNullMessage_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult(true, message: null)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_PropertyInitializer_TrueWithNullMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult { WasSuccessful = true, Message = null }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_BooleanFalseWithStringMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult(false, "Error message")|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return "Error message";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_PropertyInitializer_FalseWithStringMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COA1001:new ItemResult { WasSuccessful = false, Message = "Error message" }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return "Error message";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_BooleanTrueWithNonNullMessage_NoWarning()
    {
        // Cannot simplify if we have a meaningful message with true
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return new ItemResult(true, "Success message");
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_BooleanWithValidationIssues_NoWarning()
    {
        // Cannot simplify if we have validation issues
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.Models;
            using System.Collections.Generic;

            public class TestClass
            {
                public ItemResult GetResult()
                {
                    var issues = new List<ValidationIssue>();
                    return new ItemResult(false, "Error", issues);
                }
            }
            """);
    }
    [Test]
    public async Task ItemResult_WithVariable_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    bool success = true;
                    return {|COA1001:new ItemResult(success)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    bool success = true;
                    return success;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_WithVariableString_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    string message = "Error";
                    return {|COA1001:new ItemResult(message)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    string message = "Error";
                    return message;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResult_EmptyConstructor_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return new ItemResult();
                }
            }
            """);
    }

    [Test]
    public async Task NonItemResult_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class ItemResult
            {
                public ItemResult(bool success) { }
            }

            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return new ItemResult(true);
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_ObjectValue_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return new ItemResult<string>("result value");
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultString_BooleanConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return {|COA1001:new ItemResult<string>(true)|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultString_PropertyInitializer_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return {|COA1001:new ItemResult<string> { WasSuccessful = true }|};
                }
            }
            """, """
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return true;
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_MultiArgumentWithNonDefaultMessage_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return new ItemResult<int>(true, "Success message", 42);
                }
            }
            """);
    }

    [Test]
    public async Task NonTargetTypedExpression_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public void GetResult()
                {
                    var result = new ItemResult<int>(42);
                }
            }
            """);
    }

    [Test]
    public async Task WrappedNonTargetTypedExpression_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public Task GetResult()
                {
                    return Task.FromResult(new ItemResult<int>(42));
                }
            }
            """);
    }

    [Test]
    public async Task ArgumentToObjectParameter_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public object GetResult()
                {
                    string error = "error";
                    return BadRequest(new ItemResult(error));
                }

                private object BadRequest(object obj) => obj;
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_PropertyInitializer_ErrorMessageWithObjectNull_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            using System.Net.Mail;
            public class TestClass
            {
                public ItemResult<MailAddress> GetResult()
                {
                    var to = "user@example.com";
                    return {|COA1001:new ItemResult<MailAddress>()
                    {
                        WasSuccessful = false,
                        Message = $"Unable to send email to {to}.",
                        Object = null
                    }|};
                }
            }
            """, """
            using System.Net.Mail;
            public class TestClass
            {
                public ItemResult<MailAddress> GetResult()
                {
                    var to = "user@example.com";
                    return $"Unable to send email to {to}.";
                }
            }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_Interface_ObjectValue_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult<IBar> GetResult()
                {
                    Bar bar1 = new();
                    IBar bar2 = bar1;
                    return new ItemResult<IBar>(bar2);
                }
            }

            public interface IBar { }
            public class Bar : IBar { }
            """);
    }

    [Test]
    public async Task ItemResultGeneric_Interface_PropertyInitializer_ObjectValue_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult<IBar> GetResult()
                {
                    Bar bar1 = new();
                    IBar bar2 = bar1;
                    return new ItemResult<IBar> { Object = bar2 };
                }
            }

            public interface IBar { }
            public class Bar : IBar { }
            """);
    }
}

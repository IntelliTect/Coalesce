namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce1001_SimplifyItemResultTests : CSharpAnalyzerVerifier<Coalesce1001_SimplifyItemResult>
{
    [Fact]
    public async Task ItemResult_BooleanConstructor_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult(true)|};
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

    [Fact]
    public async Task ItemResult_PropertyInitializer_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult { WasSuccessful = true }|};
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

    [Fact]
    public async Task ItemResult_BooleanConstructor_False_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult(false)|};
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

    [Fact]
    public async Task ItemResult_PropertyInitializer_False_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult { WasSuccessful = false }|};
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

    [Fact]
    public async Task ItemResult_StringConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult("Error message")|};
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

    [Fact]
    public async Task ItemResult_PropertyInitializer_ErrorMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult { WasSuccessful = false, Message = "Error message" }|};
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

    [Fact]
    public async Task ItemResultGeneric_StringConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<int>("Error message")|};
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

    [Fact]
    public async Task ItemResultGeneric_PropertyInitializer_ErrorMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<int> { WasSuccessful = false, Message = "Error message" }|};
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

    [Fact]
    public async Task ItemResultGeneric_ObjectConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<int>(42)|};
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

    [Fact]
    public async Task ItemResultGeneric_PropertyInitializer_Object_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<int> { Object = 42 }|};
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

    [Fact]
    public async Task ItemResultGeneric_ObjectVariable_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    var value = 42;
                    return {|COALESCE1001:new ItemResult<int>(value)|};
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

    [Fact]
    public async Task ItemResultGeneric_PropertyInitializer_ObjectVariable_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    var value = 42;
                    return {|COALESCE1001:new ItemResult<int> { Object = value }|};
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

    [Fact]
    public async Task ItemResultGeneric_MultiArgumentWithObject_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<int>(true, null, 42)|};
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

    [Fact]
    public async Task ItemResultGeneric_PropertyInitializer_SuccessWithObject_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<int> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<int> { WasSuccessful = true, Object = 42 }|};
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

    [Fact]
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

    [Fact]
    public async Task ItemResult_BooleanWithNullMessage_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult(true, message: null)|};
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

    [Fact]
    public async Task ItemResult_PropertyInitializer_TrueWithNullMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult { WasSuccessful = true, Message = null }|};
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

    [Fact]
    public async Task ItemResult_BooleanFalseWithStringMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult(false, "Error message")|};
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

    [Fact]
    public async Task ItemResult_PropertyInitializer_FalseWithStringMessage_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    return {|COALESCE1001:new ItemResult { WasSuccessful = false, Message = "Error message" }|};
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task ItemResult_WithVariable_NoWarning()
    {
        // Don't suggest simplification for variables
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    bool success = true;
                    return new ItemResult(success);
                }
            }
            """);
    }

    [Fact]
    public async Task ItemResult_WithVariableString_NoWarning()
    {
        // Don't suggest simplification for variables
        await VerifyAnalyzerAsync("""
            public class TestClass
            {
                public ItemResult GetResult()
                {
                    string message = "Error";
                    return new ItemResult(message);
                }
            }
            """);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task ItemResultString_BooleanConstructor_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<string>(true)|};
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

    [Fact]
    public async Task ItemResultString_PropertyInitializer_True_ReportsInfo()
    {
        await VerifyAnalyzerAndCodeFixAsync<Coalesce1001_SimplifyItemResultCodeFixProvider>("""
            public class TestClass
            {
                public ItemResult<string> GetResult()
                {
                    return {|COALESCE1001:new ItemResult<string> { WasSuccessful = true }|};
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

    [Fact]
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
}

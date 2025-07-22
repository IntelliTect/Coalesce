namespace IntelliTect.Coalesce.Analyzer.Tests;

public class Coalesce0001_InvalidPermissionLevelTests : CSharpAnalyzerVerifier<Coalesce0001_InvalidPermissionLevel>
{
    [Fact]
    public async Task PropertyWithPermissionLevelNamedArgument_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [{|COALESCE0001:Read(PermissionLevel = SecurityPermissionLevels.DenyAll)|}]
                public string TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task PropertyWithPermissionLevelConstructorArgument_ReportsWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [{|COALESCE0001:Edit(SecurityPermissionLevels.AllowAll)|}]
                public string TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task PropertyWithRolesOnly_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            public class TestClass
            {
                [Read("Admin", "User")]
                public string TestProperty { get; set; }

                [Edit(Roles = "Admin")]
                public string AnotherProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task ClassWithPermissionLevel_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using IntelliTect.Coalesce.DataAnnotations;

            [Read(PermissionLevel = SecurityPermissionLevels.AllowAll)]
            [Edit(SecurityPermissionLevels.DenyAll)]
            public class TestClass
            {
                public string TestProperty { get; set; }
            }
            """);
    }

    [Fact]
    public async Task NonCoalesceAttribute_NoWarning()
    {
        await VerifyAnalyzerAsync("""
            using System;

            public class ReadAttribute : Attribute
            {
                public object PermissionLevel { get; set; }
            }

            public class TestClass
            {
                [Read(PermissionLevel = null)]
                public string TestProperty { get; set; }
            }
            """);
    }
}

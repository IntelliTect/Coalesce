namespace IntelliTect.Coalesce.Analyzer.Tests;

public class InvalidBehaviorsOverrideWithDenyAllTests : CSharpAnalyzerVerifier<InvalidBehaviorsOverrideWithDenyAllAnalyzer>
{
    [Fact]
    public async Task SaveMethodOverridesWithOnlyCreateDenied_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Create(SecurityPermissionLevels.DenyAll)]
            public class TestModel
            {
                public int Id { get; set; }
                
                public class TestModelBehaviors : StandardBehaviors<TestModel, DbContext>
                {
                    public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                    
                    public override Task ExecuteSaveAsync(SaveKind kind, TestModel? oldItem, TestModel item)
                    {
                        return Task.CompletedTask;
                    }
                    
                    public override Task ExecuteDeleteAsync(TestModel item) => Task.CompletedTask;
                }
            }
            """);
    }

    [Fact]
    public async Task SaveMethodOverridesWithCreateEditDenied_ReportsErrors()
    {
        await VerifyAnalyzerAsync("""
            [Create(SecurityPermissionLevels.DenyAll)]
            [Edit(SecurityPermissionLevels.DenyAll)]
            public class TestModel
            {
                public int Id { get; set; }
                
                public class TestModelBehaviors : StandardBehaviors<TestModel, DbContext>
                {
                    public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                    
                    public override ItemResult {|COA0010:BeforeSave|}(SaveKind kind, TestModel? oldItem, TestModel item)
                    {
                        return true;
                    }
                    
                    public override Task<ItemResult> {|COA0010:BeforeSaveAsync|}(SaveKind kind, TestModel? oldItem, TestModel item)
                    {
                        return Task.FromResult<ItemResult>(true);
                    }
                    
                    public override Task<ItemResult<TestModel>> {|COA0010:AfterSaveAsync|}(SaveKind kind, TestModel? oldItem, TestModel item)
                    {
                        return Task.FromResult<ItemResult<TestModel>>(true);
                    }
                    
                    public override Task {|COA0010:ExecuteSaveAsync|}(SaveKind kind, TestModel? oldItem, TestModel item) => Task.CompletedTask;
                    public override Task ExecuteDeleteAsync(TestModel item) => Task.CompletedTask;
                }
            }
            """);
    }

    [Fact]
    public async Task DeleteMethodOverrideWithDeleteAllowed_NoError()
    {
        await VerifyAnalyzerAsync("""
            public class TestModel
            {
                public int Id { get; set; }
                
                public class TestModelBehaviors : StandardBehaviors<TestModel, DbContext>
                {
                    public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                    
                    public override Task ExecuteSaveAsync(SaveKind kind, TestModel? oldItem, TestModel item) => Task.CompletedTask;
                    
                    public override Task ExecuteDeleteAsync(TestModel item)
                    {
                        return Task.CompletedTask;
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task DeleteMethodOverridesWithDeleteDenied_ReportsErrors()
    {
        await VerifyAnalyzerAsync("""
            [Delete(SecurityPermissionLevels.DenyAll)]
            public class TestModel
            {
                public int Id { get; set; }
                
                public class TestModelBehaviors : StandardBehaviors<TestModel, DbContext>
                {
                    public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                    
                    public override ItemResult {|COA0011:BeforeDelete|}(TestModel item)
                    {
                        return true;
                    }
                    
                    public override Task<ItemResult> {|COA0011:BeforeDeleteAsync|}(TestModel item)
                    {
                        return Task.FromResult<ItemResult>(true);
                    }
                    
                    public override Task<ItemResult<TestModel>> {|COA0011:AfterDeleteAsync|}(TestModel item)
                    {
                        return Task.FromResult<ItemResult<TestModel>>(true);
                    }
                    
                    public override Task ExecuteSaveAsync(SaveKind kind, TestModel? oldItem, TestModel item) => Task.CompletedTask;
                    public override Task {|COA0011:ExecuteDeleteAsync|}(TestModel item) => Task.CompletedTask;
                }
            }
            """);
    }

    [Fact]
    public async Task NestedBehaviorsWithAllCrudDenied_ReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Create(SecurityPermissionLevels.DenyAll)]
            [Edit(SecurityPermissionLevels.DenyAll)]
            [Delete(SecurityPermissionLevels.DenyAll)]
            public class TestModel
            {
                public int Id { get; set; }
                
                public class {|COA0009:TestModelBehaviors|} : StandardBehaviors<TestModel, DbContext>
                {
                    public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                    public override Task {|COA0010:ExecuteSaveAsync|}(SaveKind kind, TestModel? oldItem, TestModel item) => Task.CompletedTask;
                    public override Task {|COA0011:ExecuteDeleteAsync|}(TestModel item) => Task.CompletedTask;
                }
            }
            """);
    }

    [Fact]
    public async Task NonNestedBehaviorsClass_StillReportsError()
    {
        await VerifyAnalyzerAsync("""
            [Create(SecurityPermissionLevels.DenyAll)]
            [Edit(SecurityPermissionLevels.DenyAll)]
            [Delete(SecurityPermissionLevels.DenyAll)]
            public class TestModel
            {
                public int Id { get; set; }
            }
            
            public class TestModelBehaviors : StandardBehaviors<TestModel, DbContext>
            {
                public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                public override Task {|COA0010:ExecuteSaveAsync|}(SaveKind kind, TestModel? oldItem, TestModel item) => Task.CompletedTask;
                public override Task {|COA0011:ExecuteDeleteAsync|}(TestModel item) => Task.CompletedTask;
            }
            """);
    }

    [Fact]
    public async Task RegularMethodNotOverride_NoError()
    {
        await VerifyAnalyzerAsync("""
            [Create(SecurityPermissionLevels.DenyAll)]
            [Edit(SecurityPermissionLevels.DenyAll)]
            public class TestModel
            {
                public int Id { get; set; }
                
                public class TestModelBehaviors : StandardBehaviors<TestModel, DbContext>
                {
                    public TestModelBehaviors(CrudContext<DbContext> context) : base(context) { }
                    
                    public void ExecuteSaveAsync()  // Not an override, just same name
                    {
                    }
                    
                    public override Task {|COA0010:ExecuteSaveAsync|}(SaveKind kind, TestModel? oldItem, TestModel item) => Task.CompletedTask;
                    public override Task ExecuteDeleteAsync(TestModel item) => Task.CompletedTask;
                }
            }
            """);
    }
}

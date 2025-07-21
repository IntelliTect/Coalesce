namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class ComplexModelDependent
    {
        public int Id { get; set; }

        // Foreign key without nav prop (discovered by ForeignKeyAttribute on the inverse collection navigation).
        public int ParentId { get; set; }

        public string Name { get; set; }

        [Coalesce]
        [SemanticKernel("SameMethodNameAsMethodOnDifferentType")]
        public CaseDtoStandalone SameMethodNameAsMethodOnDifferentType(CaseDtoStandalone input) => input;
    }
}

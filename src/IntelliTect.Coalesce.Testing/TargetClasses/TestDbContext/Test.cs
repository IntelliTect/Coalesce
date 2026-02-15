using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

// This class is deliberately named "Test", since the name "Test" also occurs in its namespace.
// This class is intended for testing resolutions to issues like https://github.com/IntelliTect/Coalesce/issues/28
public class Test
{
    public int TestId { get; set; }

    public int ComplexModelId { get; set; }
    public ComplexModel ComplexModel { get; set; }

    [ListText]
    public string TestName { get; set; }
}

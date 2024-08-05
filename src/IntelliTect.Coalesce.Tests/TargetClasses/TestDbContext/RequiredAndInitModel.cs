
namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class RequiredAndInitModel
    {
        public int Id { get; set; }

        public required string RequiredRef { get; set; }
        public required int RequiredValue { get; set; }
        public required string RequiredInitRef { get; init; }
        public required int RequiredInitValue { get; init; }
        public string InitRef { get; init; }
        public int InitValue { get; init; }
        public int? NullableInitValue { get; init; }
    }
}

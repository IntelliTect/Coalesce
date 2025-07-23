using IntelliTect.Coalesce.Models;

public class TestAnalyzer
{
    public ItemResult Test1() => new ItemResult(true);  // Should suggest: true
    public ItemResult Test2() => new ItemResult(false); // Should suggest: false
    public ItemResult Test3() => new ItemResult("Error"); // Should suggest: "Error"
    public ItemResult<int> Test4() => new ItemResult<int>(42); // Should suggest: 42
    public ItemResult<string> Test5() => new ItemResult<string>("value"); // Should NOT suggest (ambiguous)
    public ItemResult<string> Test6() => new ItemResult<string>(true); // Should suggest: true
}

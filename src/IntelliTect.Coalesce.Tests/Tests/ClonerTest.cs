using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.Tests;

public class ClonerTest
{
    [Fact]
    public void CopyTest()
    {
        var src = new TestClass
        {
            I = 34,
            S = "My String",
            C = new TestClass()
        };
        src.C.I = 12;
        src.Field = "my Field";
        var dest = src.Copy();

        Assert.Equal(src.I, dest.I);
        Assert.Equal(src.S, dest.S);
        Assert.Equal(src.Field, dest.Field);
        Assert.Same(src.C, dest.C);
    }



    public class TestClass
    {
        public string S { get; set; }
        public int I { get; set; }
        public TestClass C { get; set; }
        public double Test() { return 1.0; }

        public string Field;
    }
}

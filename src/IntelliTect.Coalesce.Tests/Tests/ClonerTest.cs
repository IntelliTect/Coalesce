using IntelliTect.Coalesce.Helpers;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests;

public class ClonerTest
{
    [Test]
    public async Task CopyTest()
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

        await Assert.That(dest.I).IsEqualTo(src.I);
        await Assert.That(dest.S).IsEqualTo(src.S);
        await Assert.That(dest.Field).IsEqualTo(src.Field);
        await Assert.That(dest.C).IsSameReferenceAs(src.C);
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

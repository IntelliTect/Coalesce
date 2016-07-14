using IntelliTect.Coalesce.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Web.Tests
{
    public class ClonerTest
    {
        [Fact]
        public void CopyTest()
        {
            var src = new TestClass();
            src.I = 34;
            src.S = "My String";
            src.C = new TestClass();
            src.C.I = 12;
            src.Field = "my Field";
            var dest = src.Copy();

            Assert.Equal(src.I, dest.I);
            Assert.Equal(src.S, dest.S);
            Assert.Equal(src.Field, dest.Field);
            Assert.Same(src.C, dest.C);
        }

        [Fact]
        public void CloneTest()
        {
            var src = new TestClass();
            src.I = 34;
            src.S = "My String";
            src.C = new TestClass();
            src.C.I = 12;
            src.Field = "my Field";
            var dest = src.Clone();

            Assert.Equal(src.I, dest.I);
            Assert.Equal(src.S, dest.S);
            Assert.NotSame(src.C, dest.C);
            Assert.Equal(src.Field, dest.Field);
            Assert.Equal(src.C.I, dest.C.I);
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
}

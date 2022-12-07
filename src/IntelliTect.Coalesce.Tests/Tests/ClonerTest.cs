using IntelliTect.Coalesce.Helpers;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
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

#if NET5_0_OR_GREATER
        class NullReporter : Microsoft.EntityFrameworkCore.Design.Internal.IOperationReporter
        {
            public void WriteError(string message)
            {
            }

            public void WriteInformation(string message)
            {
            }

            public void WriteVerbose(string message)
            {
            }

            public void WriteWarning(string message)
            {
            }
        }

        [Fact]
        public void AdHoc()
        {
            var contextType = "AppDbContext";
            var assembly = Assembly.GetExecutingAssembly();
            var _contextOperations = new Microsoft.EntityFrameworkCore.Design.Internal.DbContextOperations(
                reporter: new NullReporter(),
                assembly: assembly,
                startupAssembly: assembly,
                projectDir: Environment.CurrentDirectory,
                rootNamespace: null,
                language: null,
                nullable: false,
                args: null);

            using var context = _contextOperations.CreateContext(contextType);
            var model = context.Model;
            var relationalModel = context.Model.GetRelationalModel();
        }
#endif
    }
}

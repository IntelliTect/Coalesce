using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Api
{
    public class CsvHelperTests
    {
        [Fact]
        public void CreateCsv_DoesntBlowUpOnRecursiveReferences()
        {
            // Using the standard automapping feature of CsvHelper 12.x will cause a stack overflow
            // in some situations when there are recursive properties. 
            // So, we reworked Coalesce to create the mapping manually.

            var foo = new Foo
            {
                FooId = 1,
                Name = "steve",
                Date = new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-8)),
                IsTrue = false,
                Foos = new[] { new Foo() },
                Bar = new Bar { Foo = new Foo() } 
            };

            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(new[]{ foo });

            Assert.Contains(nameof(Foo.FooId), csv);
            Assert.Contains(nameof(Foo.Name), csv);
            Assert.Contains(nameof(Foo.Date), csv);
            Assert.Contains(nameof(Foo.IsTrue), csv);
            Assert.DoesNotContain(nameof(Foo.Foos), csv);
            Assert.DoesNotContain(nameof(Foo.Bar), csv);

            Assert.Contains("1", csv);
            Assert.Contains("steve", csv);
            Assert.Contains("8/1/2017 12:00:00 AM -08:00", csv);

            var newFoo = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<Foo>(csv, true).Single();

            Assert.Equal(foo.FooId, newFoo.FooId);
            Assert.Equal(foo.Name, newFoo.Name);
            Assert.Equal(foo.Date, newFoo.Date);
            Assert.Equal(foo.IsTrue, newFoo.IsTrue);
            Assert.Null(newFoo.Bar);
            Assert.Null(newFoo.Foos);

        }

        private class Foo
        {
            public int FooId { get; set; }

            public string Name { get; set; }

            public DateTimeOffset Date { get; set; }

            public bool IsTrue { get; set; }

            public ICollection<Foo> Foos { get; set; }

            /// <summary>
            /// CSV mapping needs to not blow up when recursive references are encountered.
            /// </summary>
            public Bar Bar { get; set; }
        }

        private class Bar
        {
            /// <summary>
            /// CSV mapping needs to not blow up when recursive references are encountered.
            /// </summary>
            public Foo Foo { get; set; }
        }
    }
}

using Coalesce.Domain;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Coalesce.Web.Tests
{
    public class TypeDefinitionTests
    {
        [Fact]
        public void LoadContext()
        {
            var models = ReflectionRepository.Global.AddContext<AppDbContext>();
            Assert.Equal(9, models.Count);
            Assert.Equal(5, models.Where(f=>f.HasDbSet).Count());
        }


        [Fact]
        public void Person()
        {
            var person = ReflectionRepository.Global.GetClassViewModel<Person>();
            Assert.NotNull(person);
        }
    }
}

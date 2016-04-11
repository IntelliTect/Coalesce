using Coalesce.Domain;
using Intellitect.ComponentModel.TypeDefinition;
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
            var models = ReflectionRepository.AddContext<AppContext>();
            Assert.Equal(8, models.Count);
            Assert.Equal(5, models.Where(f=>f.HasDbSet).Count());
        }


        [Fact]
        public void Person()
        {
            var person = ReflectionRepository.GetClassViewModel<Person>();
            Assert.NotNull(person);
        }
    }
}

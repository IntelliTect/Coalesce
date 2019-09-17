using IntelliTect.Coalesce.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
    public class RoleMappingTests
    {
        [Fact]
        public void Mapping()
        {
            RoleMapping.Add("test", "1");
            Assert.True(RoleMapping.Map("test").Count() == 1);
            RoleMapping.Add("test", "2");
            Assert.True(RoleMapping.Map("test").Count() == 2);
            RoleMapping.Add("test", "1");
            Assert.True(RoleMapping.Map("test").Count() == 2);
            RoleMapping.Remove("test", "1");
            Assert.True(RoleMapping.Map("test").Count() == 1);
            RoleMapping.Remove("best", "3");
            Assert.True(RoleMapping.AllMaps.Count() == 2);
        }
    }
}

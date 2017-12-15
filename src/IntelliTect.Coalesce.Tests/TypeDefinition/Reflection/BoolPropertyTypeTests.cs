using IntelliTect.Coalesce.Tests.TypeDefinition.Common;
using IntelliTect.Coalesce.Tests.TypeDefinition.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.Reflection
{
    public partial class BoolPropertyTypeTests
    {
        [Fact]
        public void IsNullable_CorrectForValueTypes()
        {
            var vm = new ReflectionClassViewModel(typeof(Bools));
            BoolPropertyAsserts.CheckNullableBoolProperties(vm);
        }


        [Fact]
        public void IsBool_CorrectForBoolProperties()
        {
            var vm = new ReflectionClassViewModel(typeof(Bools));
            BoolPropertyAsserts.CheckIsBoolForProperties(vm);
        }

     }
}

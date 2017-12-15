using IntelliTect.Coalesce.Tests.TypeDefinition.Common;
using IntelliTect.Coalesce.Tests.TypeDefinition.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.Symbol
{
    public partial class BoolPropertyTypeSymbolTests
    {
        [Fact]
        public void IsNullable_CorrectForValueTypes()
        {
            var symbol = RoslynHelper.GetSymbolFromFile(".\\TypeDefinition\\TargetClasses\\bools.cs");
            var vm = new SymbolClassViewModel(symbol);
            BoolPropertyAsserts.CheckNullableBoolProperties(vm);
        }


        [Fact]
        public void IsBool_CorrectForBoolProperties()
        {
            var symbol = RoslynHelper.GetSymbolFromFile(".\\TypeDefinition\\TargetClasses\\bools.cs");
            var vm = new SymbolClassViewModel(symbol);
            BoolPropertyAsserts.CheckIsBoolForProperties(vm);
        }
    }
}

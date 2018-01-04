using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.ClassViewModels
{
    internal class ClassViewModelDataAttribute : Xunit.Sdk.DataAttribute
    {
        private readonly Type targetClass;

        public ClassViewModelDataAttribute(Type targetClass)
        {
            this.targetClass = targetClass;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new [] { new ClassViewModelData(targetClass, typeof(ReflectionClassViewModel)) };
            yield return new [] { new ClassViewModelData(targetClass, typeof(SymbolClassViewModel)) };
        }
    }
}

using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Util
{
    internal class ClassViewModelDataAttribute : Xunit.Sdk.DataAttribute
    {
        private readonly Type targetClass;
        private readonly object[] inlineData;

        public ClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
        {
            this.targetClass = targetClass;
            this.inlineData = additionalInlineData;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new[] {
                new ClassViewModelData(targetClass, typeof(ReflectionClassViewModel))
            }.Concat(inlineData).ToArray();

            yield return new[] {
                new ClassViewModelData(targetClass, typeof(SymbolClassViewModel))
            }.Concat(inlineData).ToArray();
        }
    }
}

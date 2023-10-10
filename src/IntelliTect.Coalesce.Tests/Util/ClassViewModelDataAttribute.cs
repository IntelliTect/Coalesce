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

        protected bool reflection = true;
        protected bool symbol = true;

        public ClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
        {
            this.targetClass = targetClass;
            this.inlineData = additionalInlineData;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (reflection) yield return new[] {
                new ClassViewModelData(targetClass, typeof(ReflectionClassViewModel))
            }.Concat(inlineData).ToArray();

            if (symbol) yield return new[] {
                new ClassViewModelData(targetClass, typeof(SymbolClassViewModel))
            }.Concat(inlineData).ToArray();
        }
    }

    internal class PropertyViewModelDataAttribute : Xunit.Sdk.DataAttribute
    {
        private readonly Type targetClass;
        private readonly string propName;
        private readonly object[] inlineData;

        protected bool reflection = true;
        protected bool symbol = true;

        public PropertyViewModelDataAttribute(Type targetClass, string propName, params object[] additionalInlineData)
        {
            this.targetClass = targetClass;
            this.propName = propName;
            this.inlineData = additionalInlineData;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (reflection) yield return new[] {
                new PropertyViewModelData(targetClass, propName, typeof(ReflectionClassViewModel))
            }.Concat(inlineData).ToArray();

            if (symbol) yield return new[] {
                new PropertyViewModelData(targetClass, propName, typeof(SymbolClassViewModel))
            }.Concat(inlineData).ToArray();
        }
    }

    internal class ReflectionClassViewModelDataAttribute : ClassViewModelDataAttribute
    {
        public ReflectionClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
            : base(targetClass, additionalInlineData)
        {
            this.symbol = false;
        }
    }

    internal class SymbolClassViewModelDataAttribute : ClassViewModelDataAttribute
    {
        public SymbolClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
            : base(targetClass, additionalInlineData)
        {
            this.reflection = false;
        }
    }
}

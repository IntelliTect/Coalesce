using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class ReflectionMethodWrapper : MethodWrapper
    {
        private MethodInfo Info { get; }

        public ReflectionMethodWrapper(MethodInfo methodInfo)
        {
            Info = methodInfo;
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Info.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() => Info.HasAttribute<TAttribute>();

        public override bool IsStatic => Info.IsStatic;

        public override TypeWrapper ReturnType => new ReflectionTypeWrapper(Info.ReturnType);

        public override bool IsInternalUse => base.IsInternalUse || !Info.IsPublic;

        public override IEnumerable<ParameterViewModel> Parameters
        {
            get
            {
                var result = new List<ParameterViewModel>();
                foreach (var parameter in Info.GetParameters())
                {
                    result.Add(new ParameterViewModel( new ReflectionParameterWrapper(parameter)));
                }
                return result;
            }
        }
    }
}

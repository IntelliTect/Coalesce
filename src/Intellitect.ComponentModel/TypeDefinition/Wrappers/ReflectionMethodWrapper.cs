using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.TypeDefinition.Wrappers
{
    internal class ReflectionMethodWrapper : MethodWrapper
    {
        private MethodInfo Info { get; }

        public ReflectionMethodWrapper(MethodInfo methodInfo)
        {
            Info = methodInfo;
        }

        public override string Name { get { return Info.Name; } }

        public override string Comment { get { return ""; } }

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Info.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Info.HasAttribute<TAttribute>();
        }

        public override bool IsStatic { get { return Info.IsStatic; } }

        public override TypeWrapper ReturnType { get { return new ReflectionTypeWrapper(Info.ReturnType); } }

        public override IEnumerable<ParameterViewModel> Parameters
        {
            get
            {
                var result = new List<ParameterViewModel>();
                foreach (var parameter in Info.GetParameters())
                {
                    result.Add(new ParameterViewModel(parameter.Name, new TypeViewModel(new ReflectionTypeWrapper(parameter.ParameterType))));
                }
                return result;
            }
        }
    }
}

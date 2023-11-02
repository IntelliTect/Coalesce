using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionMethodViewModel : MethodViewModel
    {
        private MethodBase Info { get; }

        public ReflectionMethodViewModel(ClassViewModel parent, MethodBase methodInfo) : base(parent)
        {
            Info = methodInfo;
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        public override IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
            => Info.GetAttributes<TAttribute>();

        public override bool IsStatic => Info.IsStatic;

        public override TypeViewModel ReturnType => ReflectionTypeViewModel.GetOrCreate(Parent.ReflectionRepository, MethodInfo.ReturnType);

        public override bool IsInternalUse => base.IsInternalUse || !Info.IsPublic;

        public override MethodInfo MethodInfo => (MethodInfo)Info;

        public override IEnumerable<ParameterViewModel> Parameters
            => Info.GetParameters().Select(p => new ReflectionParameterViewModel(this, p));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionClassViewModel : ClassViewModel
    {
        protected Type Info { get; }

        public ReflectionClassViewModel(Type type)
        {
            Info = type;
            Type = new ReflectionTypeViewModel(type);
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        protected override ICollection<PropertyViewModel> RawProperties => Info.GetProperties()
            .Select((p, i) => new ReflectionPropertyViewModel(this, p){ ClassFieldOrder = i })
            .Cast<PropertyViewModel>()
            .ToList();

        protected override ICollection<MethodViewModel> RawMethods => Info.GetMethods()
            .Where(m => m.IsPublic && !m.IsSpecialName)
            .Select(m => new ReflectionMethodViewModel(m, this))
            .Cast<MethodViewModel>()
            .ToList();

        protected override ICollection<TypeViewModel> RawNestedTypes => Info.GetNestedTypes()
            .Select(t => new ReflectionTypeViewModel(t))
            .Cast<TypeViewModel>()
            .ToList();
    }
}
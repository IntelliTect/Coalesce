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

        public ReflectionClassViewModel(Type type) : this(null, type)
        {
        }

        internal ReflectionClassViewModel(ReflectionRepository reflectionRepository, Type type)
        {
            Info = type;
            Type = new ReflectionTypeViewModel(reflectionRepository, type);
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        protected override IReadOnlyCollection<PropertyViewModel> RawProperties(ClassViewModel effectiveParent) => Info
            .GetProperties()
            .Select((p, i) => new ReflectionPropertyViewModel(effectiveParent, this, p){ ClassFieldOrder = i })
            .Cast<PropertyViewModel>()
            .ToList().AsReadOnly();

        protected override IReadOnlyCollection<MethodViewModel> RawMethods => Info
            .GetMethods()
            .Where(m => m.IsPublic && !m.IsSpecialName)
            .Select(m => new ReflectionMethodViewModel(this, m))
            .Cast<MethodViewModel>()
            .ToList().AsReadOnly();

        protected override IReadOnlyCollection<TypeViewModel> RawNestedTypes => Info
            .GetNestedTypes()
            .Select(t => new ReflectionTypeViewModel(ReflectionRepository, t))
            .ToList().AsReadOnly();
    }
}
using System;

#nullable enable

namespace IntelliTect.Coalesce.TypeDefinition
{
    /// <summary>
    /// Shim of a method's return value so it can be treated as an IValueViewModel.
    /// </summary>
    public class MethodReturnViewModel : ValueViewModel
    {
        internal MethodReturnViewModel(MethodViewModel method) : base(method.ResultType)
        {
            Method = method;
        }

        public override string Name => "$return";

        public override string DisplayName => "Result"; // TODO: i18n

        public override string? Description => null;

        public MethodViewModel Method { get; }

        public override bool IsRequired => false;

        public override object? GetAttributeValue<TAttribute>(string valueName)
            => Method.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>()
            => Method.HasAttribute<TAttribute>();
    }
}

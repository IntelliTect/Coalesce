using System;

#nullable enable

namespace IntelliTect.Coalesce.TypeDefinition
{
    /// <summary>
    /// Shim of a method's return value so it can be treated as an IValueViewModel.
    /// </summary>
    public class MethodReturnViewModel : IValueViewModel
    {
        internal MethodReturnViewModel(MethodViewModel method)
        {
            Method = method;
            Type = method.ResultType;
        }

        public string Name => "$return";

        public string JsVariable => Name;

        public string DisplayName => "Result"; // TODO: i18n

        public string? Description => null;

        public TypeViewModel Type { get; }

        public TypeViewModel PureType => Type.PureType;

        public MethodViewModel Method { get; }

        public bool IsRequired => false;

        public object? GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute
            => Method.GetAttributeValue<TAttribute>(valueName);

        public bool HasAttribute<TAttribute>() where TAttribute : Attribute
            => Method.HasAttribute<TAttribute>();
    }
}

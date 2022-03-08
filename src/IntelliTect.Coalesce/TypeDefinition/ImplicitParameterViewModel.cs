namespace IntelliTect.Coalesce.TypeDefinition
{
    class ImplicitParameterViewModel : ParameterViewModel
    {
        public ImplicitParameterViewModel(
            MethodViewModel parent,
            PropertyViewModel parentSourceProp,
            string name,
            string? displayName = null
        )
            : base(parent, parentSourceProp.Type)
        {
            Name = name;
            _displayName = displayName;
            ParentSourceProp = parentSourceProp;
        }

        public override string Name { get; }

        private string? _displayName;

        public override string DisplayName => _displayName ?? base.DisplayName;

        public override bool HasDefaultValue => false;
        protected override object? RawDefaultValue => null;

        public override object? GetAttributeValue<TAttribute>(string valueName) => null;
        public override bool HasAttribute<TAttribute>() => false;
    }
}

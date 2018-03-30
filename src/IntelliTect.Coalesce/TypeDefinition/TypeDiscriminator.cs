namespace IntelliTect.Coalesce.TypeDefinition
{
    public enum TypeDiscriminator
    {
        Unknown,
        Void,
        Number,
        String,
        Boolean,
        Date,
        Enum,
        Model,
        Object,
        Collection
    }

    public static class TypeDiscriminatorExtensions
    {
        public static bool IsCustomType(this TypeDiscriminator type)
        {
            return type == TypeDiscriminator.Enum || type.IsClassType();
        }

        public static bool IsClassType(this TypeDiscriminator type)
        {
            return type == TypeDiscriminator.Model || type == TypeDiscriminator.Object;
        }
    }
}

using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;

#pragma warning disable CS0618 // Type or member is obsolete

namespace IntelliTect.Coalesce.Knockout.TypeDefinition
{
    public static class ClassViewModelExtensions
    {
        /// <summary>
        /// Returns true if this class has a partial typescript file.
        /// </summary>
        public static bool HasTypeScriptPartial(this ClassViewModel cvm)
            => cvm.HasAttribute<TypeScriptPartialAttribute>();


        public static string GetViewModelGeneratedClassName(this ClassViewModel cvm)
        {
            if (!cvm.HasTypeScriptPartial())
                return cvm.ViewModelClassName;

            var name = cvm.GetAttributeValue<TypeScriptPartialAttribute>(a => a.BaseClassName);

            if (string.IsNullOrEmpty(name)) return $"{cvm.ViewModelClassName}Partial";

            return name;
        }
    }
}

using IntelliTect.Coalesce.Utilities;
using System.Linq;

namespace IntelliTect.Coalesce.TypeDefinition;

public record LiteralViewModel(TypeViewModel Type, object Value, string? Name = null)
{
    public string ValueLiteralForTypeScript(string? modelPrefix = null) => Value switch
    {
        null => "null",
        _ => Type.TsTypeKind switch
        {
            TypeDiscriminator.String => '"' + Value.ToString().EscapeStringLiteralForTypeScript() + '"',
            TypeDiscriminator.Boolean => Value.ToString()!.ToLowerInvariant(),
            TypeDiscriminator.Enum => modelPrefix != null && Type.EnumValues.First(e => e.Value.Equals(Value)) is EnumMember em
                ? modelPrefix + Type.ClientTypeName + "." + em.Name
                : Value.ToString()!,
            _ => Value.ToString()!,
        }
    };
}

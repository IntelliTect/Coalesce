namespace IntelliTect.Coalesce.TypeDefinition;

public class EnumMember
{
    public EnumMember(
        string name, 
        object value, 
        string displayName,
        string? description,
        string? comment = null
    )
    {
        Name = name;
        DisplayName = displayName;
        Value = value;
        Description = description;
        Comment = comment;
    }

    public string Name { get; }
    public object Value { get; }
    public string DisplayName { get; }
    public string? Description { get; }
    public string? Comment { get; }
}

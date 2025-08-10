using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StringSerializedEnum
{
    [Display(Name = "First Value")]
    FirstValue = 1,
    
    [Display(Name = "Second Value")]
    SecondValue = 2,
    
    ThirdValue = 3
}

public enum RegularEnum
{
    FirstValue = 1,
    SecondValue = 2,
    ThirdValue = 3
}

public class StringEnumModel
{
    public int Id { get; set; }
    
    public StringSerializedEnum StringEnum { get; set; }
    
    public RegularEnum RegularEnum { get; set; }
    
    public StringSerializedEnum? NullableStringEnum { get; set; }
}
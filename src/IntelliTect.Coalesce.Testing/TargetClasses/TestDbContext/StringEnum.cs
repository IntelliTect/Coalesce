using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StringSerializedEnum
{
    [Display(Name = "First Value")]
    FirstValue = 1,

    [Display(Name = "Second Value")]
    SecondValue = 2,

    ThirdValue = 3
}

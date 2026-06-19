using System.Collections.Generic;

#nullable enable

namespace IntelliTect.Coalesce.Testing.TargetClasses;

public class Dictionaries
{
    public IDictionary<string, int> StringIntDict { get; set; } = null!;
    public IDictionary<string, string> StringStringDict { get; set; } = null!;
    public Dictionary<string, double> StringDoubleDict { get; set; } = null!;
    public IDictionary<string, object> StringObjectDict { get; set; } = null!;
    public IDictionary<string, int>? NullableDict { get; set; }
}

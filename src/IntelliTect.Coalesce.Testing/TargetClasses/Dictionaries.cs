using System.Collections.Generic;

#nullable enable

namespace IntelliTect.Coalesce.Testing.TargetClasses;

public class Dictionaries
{
    public IDictionary<string, int> StringIntDict { get; set; } = new Dictionary<string, int>();
    public IDictionary<string, string> StringStringDict { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, double> StringDoubleDict { get; set; } = new Dictionary<string, double>();
    public IDictionary<string, object> StringObjectDict { get; set; } = new Dictionary<string, object>();
    public IDictionary<string, int>? NullableDict { get; set; }
    public IDictionary<int, string> IntStringDict { get; set; } = new Dictionary<int, string>();
}

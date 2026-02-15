using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

public class Bools
{
    public bool NonNullableKeywordName { get; set; }
    public Boolean NonNullableClassName { get; set; }
    public bool? NullableQuestionMarkKeywordName { get; set; }
    public Boolean? NullableQuestionMarkClassName { get; set; }
    public Nullable<bool> NullableGenericKeywordName { get; set; }
    public Nullable<Boolean> NullableGenericClassName { get; set; }

    public bool[] Array { get; set; }
    public bool?[] ArrayNullable { get; set; }
    public ICollection<bool> Collection { get; set; }
    public ICollection<bool?> CollectionNullableItems { get; set; }
}

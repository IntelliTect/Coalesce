using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses;

public class OrderingChild
{
    public int Id { get; set; }

    public int OrderingParent2Id { get; set; }
    [DefaultOrderBy(FieldOrder = 2)]
    public OrderingParent OrderingParent2 { get; set; }

    public int OrderingParent1Id { get; set; }
    [DefaultOrderBy(FieldOrder = 1)]
    public OrderingParent OrderingParent1 { get; set; }
}

public class OrderingParent
{
    public int Id { get; set; }

    public int OrderingGrandparentId { get; set; }
    [DefaultOrderBy]
    public OrderingGrandparent OrderingGrandparent { get; set; }
}

public class OrderingGrandparent
{
    public int Id { get; set; }

    [DefaultOrderBy]
    public string OrderedField { get; set; }
}



public class OrdersByUnorderableParent
{
    public int Id { get; set; }

    [DefaultOrderBy]
    public UnspecifiedParent Parent { get; set; }
}

public class UnspecifiedParent
{
    public string Field { get; set; }
}

public class SuppressedDefaultOrdering
{
    [DefaultOrderBy(Suppress = true)]
    public int Id { get; set; }

    [DefaultOrderBy(Suppress = true)]
    public string Name { get; set; }
}

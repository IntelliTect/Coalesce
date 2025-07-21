namespace Coalesce.Domain;

public class BaseClass
{
    public int Id { get; set; }

    public string? BaseClassString { get; set; }
}

public class BaseClassDerived : BaseClass
{
    public string? DerivedClassString { get; set; }
}

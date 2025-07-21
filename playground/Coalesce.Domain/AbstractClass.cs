using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace Coalesce.Domain;

public abstract class AbstractClass
{
    public int Id { get; set; }

    public string? AbstractClassString { get; set; }

    [ManyToMany("People")]
    public List<AbstractClassPerson>? AbstractModelPeople { get; set; }

    [Coalesce]
    public int GetId() => Id;

    [Coalesce]
    public static int GetCount(AppDbContext db) => db.AbstractClasses.Count();

    [Coalesce]
    public static AbstractClass EchoAbstractModel(AbstractClass model) => model;
}

public class AbstractClassImpl : AbstractClass
{
    public string? ImplString { get; set; }
}

public class AbstractClassPerson
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public int AbstractClassId { get; set; }
    public AbstractClass? AbstractClass { get; set; }
}   

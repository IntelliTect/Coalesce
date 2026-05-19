using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api.DataSources;
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

    /// <summary>
    /// A default data source declared with an open generic parameter constrained to <see cref="AbstractClass"/>.
    /// Because <typeparamref name="T"/> is constrained to <see cref="AbstractClass"/>,
    /// this data source is automatically used as the default for <see cref="AbstractClass"/>
    /// and every derived type (e.g. <see cref="AbstractClassImpl"/>) without needing to
    /// declare a separate data source on each derived class.
    /// </summary>
    [DefaultDataSource]
    public class DefaultSource<T>(CrudContext<AppDbContext> context)
        : StandardDataSource<T, AppDbContext>(context)
        where T : AbstractClass
    {
        public override IQueryable<T> GetQuery(IDataSourceParameters parameters)
            => base.GetQuery(parameters).OrderByDescending(e => e.Id);
    }
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

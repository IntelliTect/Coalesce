using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

// Attributes defined on the base class to test attribute inheritance
[Read(Roles = "ReadRole")]
[Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
[Edit(PermissionLevel = SecurityPermissionLevels.AllowAll)]
public abstract class AbstractModel
{
    public int Id { get; set; }

    public string Discriminator { get; set; }

    [ManyToMany("People")]
    public List<AbstractModelPerson> AbstractModelPeople { get; set; }

    [Coalesce]
    public int GetId() => Id;

    [Coalesce]
    public static int GetCount(AppDbContext db) => db.AbstractModels.Count();

    [Coalesce]
    public static AbstractModel EchoAbstractModel(AbstractModel model) => model;

    /// <summary>
    /// An open generic data source constrained to AbstractModel.
    /// Should be discovered for AbstractModel itself and all derived types (AbstractImpl1, AbstractImpl2).
    /// </summary>
    public class AbstractModelDataSource<T>(CrudContext<AppDbContext> context)
        : StandardDataSource<T, AppDbContext>(context)
        where T : AbstractModel;

    /// <summary>
    /// An open generic default data source constrained to AbstractModel.
    /// Should be the default data source for AbstractModel and all derived types.
    /// </summary>
    [DefaultDataSource]
    public class DefaultAbstractModelDataSource<T>(CrudContext<AppDbContext> context)
        : StandardDataSource<T, AppDbContext>(context)
        where T : AbstractModel;
}

[Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
public class AbstractImpl1 : AbstractModel
{
    public string Impl1OnlyField { get; set; }

    public int? ParentId { get; set; }
    public AbstractModel Parent { get; set; }

    /// <summary>
    /// Overrides the inherited open generic default data source for AbstractImpl1 only.
    /// </summary>
    [DefaultDataSource]
    public class Impl1DefaultDataSource(CrudContext<AppDbContext> context)
        : StandardDataSource<AbstractImpl1, AppDbContext>(context);

    /// <summary>
    /// Overrides the inherited open generic named "AbstractModelDataSource" for AbstractImpl1 only.
    /// </summary>
    public class AbstractModelDataSource(CrudContext<AppDbContext> context)
        : StandardDataSource<AbstractImpl1, AppDbContext>(context);
}

[Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
public class AbstractImpl2 : AbstractModel
{
    public string Impl2OnlyField { get; set; }
}

/// <summary>
/// A top-level open generic data source constrained to AbstractModel,
/// discovered via [Coalesce] attribute. Should be available for AbstractModel
/// itself and all derived types.
/// </summary>
[Coalesce]
public class TopLevelAbstractModelDataSource<T>(CrudContext<AppDbContext> context)
    : StandardDataSource<T, AppDbContext>(context)
    where T : AbstractModel;

public class AbstractModelPerson
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }

    public int AbstractModelId { get; set; }
    public AbstractModel AbstractModel { get; set; }
}

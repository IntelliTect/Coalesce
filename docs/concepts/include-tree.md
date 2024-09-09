# Include Tree

When Coalesce maps from the your POCO objects that are returned from EF Core queries, it will follow a structure called an `IncludeTree` to determine what relationships to follow and how deep to go in re-creating that structure in the mapped DTOs.


## Purpose

Without an `IncludeTree` present, Coalesce will map the entire object graph that is reachable from the root object. This can often spiral out of control if there aren't any rules defining how far to go while turning this graph into a tree.

For example, suppose you had the following model with a many-to-many relationship (key properties omitted for brevity):

``` c#
public class Employee
{
    [ManyToMany("Projects")]
    public ICollection<EmployeeProject> EmployeeProjects { get; set; }
            
    public static IQueryable<Employee> WithProjectsAndMembers(AppDbContext db, ClaimsPrincipal user)
    {
        // Load all projects of an employee, as well as all members of those projects.
        return db.Employees
            .Include(e => e.EmployeeProjects)
                .ThenInclude(ep => ep.Project.EmployeeProjects)
                .ThenInclude(ep => ep.Employee);
    }
}

public class Project
{
    [ManyToMany("Employees")]
    public ICollection<EmployeeProject> EmployeeProjects { get; set; }
}

public class EmployeeProject
{
    public Employee Employee { get; set; }
    public Project Project { get; set; }
}
```

Now, imagine that you have five employees and five projects, with every employee being a member of every project (i.e. there are 25 EmployeeProject rows).

Your client code makes a call to the Coalesce-generated API to load Employee #1 using the custom data source:

<CodeTabs>
<template #vue>

``` ts
import { Employee } from '@/viewmodels.g'
import { EmployeeViewModel } from '@/viewmodels.g'

var employee = new EmployeeViewModel();
employee.$dataSource = new Employee.DataSources.WithProjectsAndMembers();
employee.$load(1);
```

</template>
</CodeTabs>

If you're already familiar with the fact that an `IncludeTree` is implicitly created in this scenario, then imagine for a moment that this is not the case (if you're not familiar with this fact, then keep reading!).

After Coalesce has called your [Data Sources](/modeling/model-components/data-sources.md) and evaluated the EF IQueryable returned, there are now 35 objects loaded into the current `DbContext` being used to handle this request - the 5 employees, 5 projects, and 25 relationships.

To map these objects to DTOs, we start with the root (employee #1) and expand outward from there until the entire object graph has been faithfully re-created with DTO objects, including all navigation properties.

The root DTO object (employee #1) then eventually is passed to the JSON serializer by ASP.NET Core to formulate the response to the request. As the object is serialized to JSON, the only objects that are not serialized are those that were already serialized as an ancestor of itself. What this ultimately means is that the structure of the serialized JSON with our example scenario ends up following a pattern like this (the vast majority of items have been omitted):

``` :no-line-numbers
Employee#1
    EmployeeProject#1
        Project#1
            EmployeeProject#6
                Employee#2
                    EmployeeProject#7
                        Project#2
                            ... continues down through all remaining employees and projects.
                    ...
            EmployeeProject#11
                Employee#3
            ...
    EmployeeProject#2
        Project#2
    ...
```

See how the structure includes the EmployeeProjects of Employee#2? We didn't write our custom data source calls to `.Include` in such a way that indicated that we wanted the root employee, their projects, the employees of those projects, and then **the projects of those employees**. But, because the JSON serializer blindly follows the object graph, that's what gets serialized. It turns out that the depth of the tree increases on the order of `O(n^2)`, and the total size increases on the order of `Ω(n!)`.

This is where `IncludeTree` comes in. When you use a custom data source like we did above, Coalesce automatically captures the structure of the calls to `.Include` and `.ThenInclude`, and uses this to perform trimming during creation of the DTO objects.

With an `IncludeTree` in place, our new serialized structure looks like this:

``` :no-line-numbers
Employee#1
    EmployeeProject#1
        Project#1
            EmployeeProject#6
                Employee#2
            EmployeeProject#11
                Employee#3
            ...
    EmployeeProject#2
        Project#2
    ...
```

No more extra data trailing off the end of the projects' employees!


## Usage

### Custom Data Sources

In most cases, you don't have to worry about creating an `IncludeTree`. When using the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) (or a derivative), the structure of the `.Include` and `.ThenInclude` calls will be captured automatically and be turned into an `IncludeTree`. Projected queries are also supported, although very complex projections might not capture all paths - please open an issue if you find a scenario that doesn't work.

However, there are sometimes cases where you perform complex loading in these methods that involves loading data into the current `DbContext` outside of the `IQueryable` that is returned from the method. The most common situation for this is needing to conditionally load related data - for example, load all children of an object where the child has a certain value of a Status property.

In these cases, Coalesce provides a pair of extension methods, `.IncludedSeparately` and `.ThenIncluded`, that can be used to merge in the structure of the data that was loaded separately from the main `IQueryable`.

For example:

``` c#
public override IQueryable<Employee> GetQuery()
{
    // Load all projects that are complete, and their members, into the db context.
    Db.Projects
        .Include(p => p.EmployeeProjects).ThenInclude(ep => ep.Employee)
        .Where(p => p.Status == ProjectStatus.Complete)
        .Load();

    // Return an employee query, and notify Coalesce that we loaded the projects in a different query.
    return Db.Employees
        .IncludedSeparately(e => e.EmployeeProjects)
        .ThenIncluded(ep => ep.Project.EmployeeProjects)
        .ThenIncluded(ep => ep.Employee);
}
```

You can also override the `GetIncludeTree` method of the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) to achieve the same result:

``` c#
public override IncludeTree GetIncludeTree(IQueryable<T> query, IDataSourceParameters parameters) => Db
    .Employees
    .IncludedSeparately(e => e.EmployeeProjects)
    .ThenIncluded(ep => ep.Project.EmployeeProjects)
    .ThenIncluded(ep => ep.Employee)
    .GetIncludeTree(); 
```

### Model Methods

If you have [custom methods](/modeling/model-components/methods.md) that return object data, you may also want to control the structure of the returned data when it is serialized. Fortunately, you can also use `IncludeTree` in these situations. Without an `IncludeTree`, the entire object graph is traversed and serialized without limit.

::: tip
An `IncludeTree` can be obtained from any `IQueryable` by calling the `GetIncludeTree` extension method (`using IntelliTect.Coalesce.Helpers.IncludeTree`).

In situations where your root object isn't on your `DbContext` (see [External Types](/modeling/model-types/external-types.md)), you can use `Enumerable.Empty<MyNonDbClass>().AsQueryable()` to get an `IQueryable` to start from. When you do this, you **must** use `IncludedSeparately` - the regular EF `Include` method won't work without a `DbSet`.
:::

To return an `IncludeTree` from a custom method is to make that method return an `ItemResult<T>`, and then set the `IncludeTree` property of the `ItemResult` object. For example:

``` c#
public class Employee
{
    public async Task<ItemResult<ICollection<Employee>>> GetChainOfCommand(AppDbContext db)
    {
        IQueryable<Employee> query = db.Employees
            .Include(e => e.Supervisor);

        var ret = new List<Employee>();
        var current = this;
        while (current.Supervisor != null)
        {
            ret.Push(current);
            current = await query.FirstOrDefaultAsync(e => e.EmployeeId == current.SupervisorId);
        }

        return new(ret, includeTree: query.GetIncludeTree());
    }
}
```


### External Type Caveats

One important point remains regarding `IncludeTree` - it is not used to control the serialization of objects which are not mapped to the database, known as [External Types](/modeling/model-types/external-types.md). External Types are always put into the DTOs when encountered (unless otherwise prevented by [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) or [Security Attributes](/modeling/model-components/attributes/security-attribute.md)), with the assumption that because these objects are created by you (as opposed to Entity Framework), you are responsible for preventing any undesired circular references.

By not filtering unmapped properties, you as the developer don't need to account for them in every place throughout your application where they appear - instead, they 'just work' and show up on the client as expected.

Note also that this statement does not apply to database-mapped objects that hang off of unmapped objects - any time a database-mapped object appears, it will be controlled by your include tree. If no include tree is present (because nothing was specified for the unmapped property), these mapped objects hanging off of unmapped objects will be serialized freely and with all circular references, unless you include some calls to `.IncludedSeparately(m => m.MyUnmappedProperty.MyMappedProperty)` to limit those objects down.
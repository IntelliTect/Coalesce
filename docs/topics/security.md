
# Security

This page is a comprehensive overview of all the techniques that can be used in a Coalesce application to restrict the usage of  API endpoints that Coalesce generates.

[[toc]]

## Endpoint Security

Coalesce generates API endpoints by traversing your data model's classes, starting from types annotated with `[Coalesce]`. This usually includes your `DbContext` class, as well as any [Service](/modeling/model-types/services.md) classes or interfaces.

Classes can be hidden from Coalesce entirely by annotating them with `[InternalUse]`, preventing generation of API endpoints for that class, as well as preventing properties of that type from being exposed.

`DbSet<>` properties on your `DbContext` class can also be annotated with `[InternalUse]`, causing that type to be treated by Coalesce like an [External Type](/modeling/model-types/external-types.md) rather than an [Entity](/modeling/model-types/entities.md), once again preventing generation of API endpoints but _without_ preventing properties of that type from being exposed.

### Standard CRUD Endpoints 
For each of your [Entities](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md), Coalesce generates a set of CRUD API endpoints (`/get`, `/list`, `/count`, `/save`, and `/delete`). 

The default behavior is that all endpoints require an authenticated user (anonymous users are rejected).

These endpoints can be secured by placing any or all of the [[Read], [Create], [Edit], and [Delete] attributes](/modeling/model-components/attributes/security-attribute.md) on the the class. Each attribute can specify required roles for that action, or open that action to anonymous, unauthenticated users, or disable the endpoint entirely.


This security is applied to the generated [controllers](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions). The `[Read]` attribute on a class ***does not*** affect instances of that class when those instances are present as child properties of other types, since in those scenarios the data will be coming from a different endpoint on a different controller.

<table> 
<thead>
<tr>
<th>Endpoints</th>
<th>Governing Attributes</th>
</tr>
</thead>
<tr>
<td>

`/get`, `/list`, `/count`
</td>
<td>

``` c#:no-line-numbers
[ReadAttribute]
```
</td>
</tr>
<tr>
<td>

`/save`
</td>
<td>

``` c#:no-line-numbers
[CreateAttribute] // Affects saves of new entities
[EditAttribute]   // Affects saves of existing entities
```
</td>
</tr>
<tr>
<td>

`/delete`
</td>
<td>

``` c#:no-line-numbers
[DeleteAttribute]
```
</td>
</tr>
</table>

Here are some examples of applying security attributes to an entity class. If a particular action doesn't need to be restricted, you can omit that attribute, but this example shows usages of all four:

``` c#:no-line-numbers
// Allow read access by unauthenticated, anonymous users:
[Read(SecurityPermissionLevels.AllowAll)]
// Allow creation of new entities by the Admin and HR roles (params string[] style):
[Create("Admin", "HR")]
// Allow editing of existing Employee entities by users with the Admin or HR roles (CSV style):
[Edit("Admin,HR")]
// Prohibit deletion of Employee entities
[Delete(SecurityPermissionLevels.DenyAll)]
public class Employee 
{
    public int EmployeeId { get; set; }
}
```

### Custom Methods and Services

To secure the endpoints generated for your [Custom Methods](/modeling/model-components/methods.md) and [Services](/modeling/model-types/services.md), the [[Execute] attribute](/modeling/model-components/attributes/execute.md) can be used to specify a set of required roles for that endpoint, or to open that endpoint to anonymous users.

The default behavior is that all endpoints require an authenticated user (anonymous users are rejected).

For example:

``` c#:no-line-numbers
public class Employee 
{
    public int EmployeeId { get; set; }

    [Coalesce, Execute("Payroll,HR")]
    public void GiveRaise(int centsPerHour) {
        // Only Payroll and HR users can call this method
    }

    [Coalesce, Execute(SecurityPermissionLevels.AllowAll)]
    public void SendMessage(string message) {
        // Anyone (even anonymous, unauthenticated users) can call this method.
    }
}
```

## Property/Column Security


### Internal Properties

Properties can be hidden from Coalesce entirely, either with the [[InternalUse]](/modeling/model-components/attributes/internal-use.md) attribute or non-public C# access modifiers.

The properties in the following example are hidden entirely from all Coalesce functionality and generated APIs:

``` c#:no-line-numbers
using IntelliTect.Coalesce.DataAnnotations;
public class Employee 
{
  // InternalUseAttribute hides anything from Coalesce.
  [InternalUse]
  public string Name { get; set; }

  // Non-public C# access modifiers will hide properties from Coalesce:
  internal decimal Salary { get; set; }

  // Property's type is [InternalUse], so properties using that type are also internal.
  public Department Department { get; set; }
}

[InternalUse]
public class Department
{
  // All properties on an [InternalUse] type are non-exposed,
  // since the parent type is not exposed.
  public string Name { get; set; }
}
```

### Attributes
The [[Read] and [Edit] attributes](/modeling/model-components/attributes/security-attribute.md) can be placed on the properties on your [Entities](/modeling/model-types/entities.md) and [External Types](/modeling/model-types/external-types.md) to apply role-based restrictions to the usage of that property.

This security is primarily executed and enforced by the mapping that occurs in the [generated DTOs](/stacks/agnostic/dtos.md). It is also checked by the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) to prevent sorting, searching, and filtering by properties that a user is not permitted to read.


### Read-Only Properties

A property in Coalesce can be made read-only in any of the following ways:

``` c#:no-line-numbers
using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel;
public class Employee 
{
  // A property with a [Read] attribute but no [Edit] attribute is read-only:
  [Read]
  public string Name { get; set; }

  // Payroll users and HR users can read this property. Nobody can edit it:
  [Read("Payroll,HR")]
  public decimal Salary { get; set; }

  // Using System.ComponentModel.ReadOnlyAttribute:
  [ReadOnly(true)]
  public DateTime BirthDate { get; set; }

  // Non-public setter:
  public DateTime StartDate { get; internal set; }

  // Edits denied:
  [Edit(SecurityPermissionLevels.DenyAll)]
  public string EmployeeNumber { get; set; }
}
```

### Read/Write Properties

Reading and writing a property in Coalesce can be restricted by roles:

``` c#:no-line-numbers
using IntelliTect.Coalesce.DataAnnotations;
public class Employee 
{
  // A property with no attributes is readable and writable without restriction
  public string Name { get; set; }

  // When a [Read] and [Edit] attributes are both present,
  // the read roles are required for edits in addition to any edit roles.
  // Property is only readable by Payroll & HR,
  // and is also only editable by Payroll & HR.
  [Read("Payroll,HR"), Edit]
  public DateTime BirthDate { get; set; }

  // Property is readable by Payroll and HR, and editable only by Payroll.
  [Read("Payroll", "HR"), Edit("Payroll")]
  public decimal Salary { get; set; }

  // Property is readable by Payroll, and editable only by a user who is both Payroll AND HR.
  [Read("Payroll"), Edit("HR")]
  public DateTime StartDate { get; set; }

  // Init-only properties on entities can only be set by the first /save of the entity.
  public string EmployeeNumber { get; init; }
}
```

A few of the examples above point out that when a property is restricted for reading by roles, 
those roles are also required when editing that property. This is because it usually doesn't make sense 
for a user to change a value when they have no way of knowing what the original value was.
If you have a situation where a property should be editable without knowing the original value,
use a custom method on the model to accept and set the new value.


## Row-level Security

### Data Sources

In Coalesce, [Data Sources](/modeling/model-components/data-sources.md) are the mechanism that you can extend to implement row-level security on your [Entities](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md).

Data Sources are used when fetching results for `/get`, `/list`, and `/count` endpoints, and when fetching the target of a `/save` or `/delete`, and when fetching the invocation target of an [Instance Method](/modeling/model-components/methods.md#instance-methods). 

By default, your entities will be fetched using the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source), but you can declare a custom default data source for each of your entities to override this default functionality.

For most use cases, all your security rules will be implemented in the [GetQuery/GetQueryAsync](/modeling/model-components/data-sources.md#member-getquery) method. This is the most foundational method of the data source that all other functions in the data source build upon. Any predicates applied to the query of a type's default data source will affect all of the type's generated API endpoints (except for static custom methods).

There are a few different techniques that you can use to apply filtering in a data source, each one working for a specific use case. The example below includes an example of each technique.

#### Query Predicates
The **Query Predicates** technique involves applying a `.Where()` predicate to your query to filter the root entities that are returned by the query using some database-executed logic. This is a form of row-level security and can be used to only include a record based on the values of that record in the database.


#### Conditional Includes
The **Conditional Includes** technique involves only appending `.Include()` calls to your query if some server-executed criteria is met. Usually this involves checking the roles of a user and only including a navigation property if the user is in the requisite role. This technique cannot be used with database-executed logic and is therefore a form of table-level security, not row-level security.

#### Filtered Includes
The **Filtered Includes** technique involves using [EF Core filtered includes](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include) to apply database-executed logic to filter the rows of child collection navigation properties. 

EF filtered Includes **cannot** be used to apply database-executed filters to *reference* navigation properties due to [lack of EF support](https://github.com/dotnet/efcore/issues/24422) - see the sections below on [transform results](#transform-results) and [global query filters](#ef-global-query-filters) for two possible solutions.


A complex example using all three of the above techniques:

``` c#:no-line-numbers
public class Employee 
{
  public int EmployeeId { get; set; }
  public bool IsIntern { get; set; }
  public List<DepartmentMember> DepartmentMembers { get; set; }

  // Override the default data source for Employee with a custom one:
  [DefaultDataSource]
  public class DefaultSource : StandardDataSource<Employee, AppDbContext>
  {
    public DefaultSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Employee> GetQuery(IDataSourceParameters parameters) {
      IQueryable<Employee> query = Db.Employees;

      // TECHNIQUE: Conditional Includes - subset child objects using server-executed logic:
      if (User.IsInRole("HR")) {
        // HR can see everything. Return early so they are not subjected to the other filters:
        return query.Include(e => e.DepartmentMembers).ThenInclude(dm => dm.Department);
      }

      // TECHNIQUE: Query Predicates - subset root objects using database-executed logic:
      int employeeId = User.GetEmployeeId();
      query = query.Where(e => 
          // Anyone can see interns
          e.IsIntern ||
          // Otherwise, a user can only see employees in their own departments:
          e.DepartmentMembers.Any(dm => dm.Department.DepartmentMembers.Any(u => u.EmployeeId == employeeId))
        );

      // TECHNIQUE: EF Core Filtered Includes - subset collections using database-executed logic.
      // Include the departments of employees, but only those that the current user is a member of.
      query = query.Include(e => e.DepartmentMembers
        .Where(dm => dm.Department.DepartmentMembers.Any(u => u.EmployeeId == employeeId)))
        .ThenInclude(dm => dm.Department);
      
      return query;
    }
  }
}

public class Department 
{
  public int DepartmentId { get; set; }
  public string Name { get; set; }
  public List<DepartmentMember> DepartmentMembers { get; set; }

  // Override the default data source for Department with a custom one:
  [DefaultDataSource]
  public class DefaultSource : StandardDataSource<Department, AppDbContext>
  {
    public DefaultSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Department> GetQuery(IDataSourceParameters parameters) {
      IQueryable<Department> query = Db.Departments
        .Include(e => e.DepartmentMembers).ThenInclude(dm => dm.Employee);

      if (!User.IsInRole("HR")) 
      {
        // Non-HR users can only see their own departments:
        query = query.Where(d => d.DepartmentMembers.Any(dm => dm.EmployeeId == User.GetEmployeeId()));
      }

      return query;
    }
  }
}

// Only HR can directly read or modify DepartmentMember records.
[Read("HR"), Create("HR"), Edit("HR"), Delete("HR")]
public class DepartmentMember 
{
  public int Id { get; set; }

  public int DepartmentId { get; set; }
  public Department Department { get; set; }
  public int EmployeeId { get; set; }
  public Employee Employee { get; set; }
}

```

#### Transform Results

There exists a fourth technique in Data Sources for applying filtered includes: the [TransformResultsAsync](/modeling/model-components/data-sources.md#member-transformresults) method. Unlike the other techniques above that are performed in the `GetQuery` method and applied at the beginning of the data source query pipeline, `TransformResults` is applied at the very end of the process against the materialized results. It also only affects the responses from the generated `/get`, `/list`, `/save`, and `/delete` endpoints - it has no bearing on the invocation target of [instance methods](/modeling/model-components/methods.md#instance-methods).

The primary purpose of `TransformResults` is to conditionally load navigation properties. This was very useful before EF Core introduced native [filtered includes](#filtered-includes) for collection navigation properties, and is still useful for applying filtered includes to *reference* navigation properties since EF [does not support this](https://github.com/dotnet/efcore/issues/24422). It can also be used for any kind of filtered includes if native EF filtered includes get translated into poorly-performant SQL, or it can be used to populate [external type](/modeling/model-types/external-types.md) or other non-database-mapped properties on your entities.

The general technique for using `TransformResults` involves using [EF Core Explicit Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/explicit#explicit-loading) to attach additional navigation properties to the result set, and then using Coalesce's `.IncludedSeparately()` method in the data source's `GetQuery` so that Coalesce can still build the correct [Include Tree](/concepts/include-tree.md) to shape the serialization of your results.


``` c#:no-line-numbers
public class Employee 
{
  public int EmployeeId { get; set; }
  public int ManagerId { get; set; }
  public Employee Manager { get; set; }

  [DefaultDataSource]
  public class DefaultSource : StandardDataSource<Employee, AppDbContext>
  {
    public DefaultSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Employee> GetQuery(IDataSourceParameters parameters) 
      // Use IncludedSeparately to instruct Coalesce that we're going to 
      // manually populate the Manager, and that it should be mapped to the result DTOs
      // despite not being eagerly loaded with EF's .Include() method.
      => Db.Employees.IncludedSeparately(e => e.Manager);

    public override async Task TransformResultsAsync(
      IReadOnlyList<Employee> results,
      IDataSourceParameters parameters
    )
    {
      foreach (var employee in results)
      {
        // Only load the employee's manager if the current logged in user is that manager.
        if (employee.ManagerId == User.GetEmployeeId() && employee.Manager is null) {
          await Db.Employees.Where(e => e.EmployeeId == employee.ManagerId).LoadAsync();
        }
      }
    }
  }
}
```

For a more complete explanation of everything you can do with data sources, see the full [Data Sources](/modeling/model-components/data-sources.md) documentation page.

### EF Global Query Filters

Since Coalesce's data access layer is built on top of Entity Framework, you can also use [Entity Framework's Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters) feature to apply row-level security.

This approach is less flexible than custom Coalesce data sources and has other [drawbacks](https://learn.microsoft.com/en-us/ef/core/querying/filters#accessing-entity-with-query-filter-using-required-navigation) as well, but on the other hand it has more absolute authority, is less susceptible to issues like inadvertently returning data through unfiltered navigation properties, and can sometimes require less work to implement than individual data sources.

Global Query Filters are also the only way to implement database-executed [filtered includes](#filtered-includes) of [reference navigation properties](https://learn.microsoft.com/en-us/ef/core/modeling/relationships#definition-of-terms), as there is no version of `.Include()` for reference navigation properties that allows a database-executed predicate to be applied. See [this open issue](https://github.com/dotnet/efcore/issues/24422) on EF Core.



### Foreign Key Injection Vulnerabilities

When a user is saving a model with Coalesce, they can provide values for the model's foreign key properties. When this interaction takes place through a user interface, the user is not likely to produce a foreign key referencing an object that the user is not allowed to view.

A malicious user, however, is a different story. Imagine a user who is brute-forcing the `/save` endpoint on one of your entities, enumerating values of a foreign key. The may be trying to leak data through navigation property values returned by the response from the save, or they may be trying to inject their data into an object graph that they do not otherwise have access to.

If this scenario sounds like a plausible threat vector your application, be sure to perform sufficient [validation](#data-validation) of incoming foreign keys to ensure that the user is allowed to use a particular foreign key value before saving it to your database.

Also consider making any required foreign keys that should not change for the lifetime of an entity into init-only properties (i.e. use the `init` accessor in C# instead of the `set` accessor). While this does not entirely solve the foreign key injection issue, it eliminates the need to validate that a user is not changing the parent of an object if such an operation is not desirable.



## Data Validation

Coalesce does not perform any server-side validation of incoming data. Your database will of course enforce any constraints (referential integrity, `not null`, etc.), but for anything else, you must implement this yourself in the appropriate locations:

### Saves and Deletes

Validation of `/save` and `/delete` actions against [Entities](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md) are performed by the [Behaviors](/modeling/model-components/behaviors.md) for the type. Behaviors can be overridden to perform validation and other customization of the save and delete process, as in the following example:


``` c#
public class Employee 
{
  public int IsCeo { get; set; }
  public decimal Salary { get; set; }

  [Coalesce]
  public class Behaviors : StandardBehaviors<Employee, AppDbContext>
  {
    public Behaviors(CrudContext<AppDbContext> context) : base(context) { }

    public override ItemResult BeforeSave(SaveKind kind, Employee? oldItem, Employee item)
    {
      // `oldItem` is a shallow copy of entity from the database,
      // and `item` is the tracked entity with incoming user data applied to it.
      if (item.Salary > 1000000m && !oldItem.IsCeo) return "Salary is too high.";
      return true;
    }

    public override ItemResult BeforeDelete(Case item)
    {
      if (item.IsCeo) return "The CEO cannot be fired.";
      return true;
    }
  }
}
```

### Custom Methods and Services

For [Custom Methods](/modeling/model-components/methods.md) and [Services](/modeling/model-types/services.md), perform your own validation and return errors when validation fails. Custom methods that need to return errors to the client are recommended to wrap their return type in an `ItemResult<T>`, allowing errors to be received and handled elegantly by your Coalesce Typescript code.

``` c#
public class Employee 
{
  public decimal Salary { get; set; }

  [Coalesce]
  public ItemResult<decimal> GiveRaise(decimal raiseAmount)
  {
    if (raiseAmount > 3.5m) return "Raises must be less than $3.50."
    Salary += raiseAmount;
    return Salary;
  }
}
```

## Security Overview Page

Coalesce provides batteries-included page that you can view to review the effective security rules in place for all the Coalesce-generated code in your project. Add this page to your application by mapping it as a route, either directly on `WebHost` in .NET 6+, or in `UseEndpoints` for 3.1+.

::: tip
  If you include the security overview in your production app, you should secure it with an authorization policy like in the example below.
  Alternatively, only map the endpoint in non-production environments.
:::
``` c#
// .NET 6+ Program.cs:
app.MapCoalesceSecurityOverview("coalesce-security").RequireAuthorization(
    new AuthorizeAttribute { Roles = env.IsDevelopment() ? null : "Admin" }
);

// .NET Core 3.1+ Startup.cs:
app.UseEndpoints(endpoints =>
{
    endpoints.MapCoalesceSecurityOverview("coalesce-security").RequireAuthorization(
        new AuthorizeAttribute { Roles = env.IsDevelopment() ? null : "Admin" }
    );
});
```

Example of the contents of the security overview page:
![](./security-overview.webp)

## Testing Your Security

If your application has complex security requirements and/or sensitive data that needs to be protected, you are encouraged to invest time into creating a set of automated tests to ensure that it is working how you expect. 

The most comprehensive way to do this is to build a suite of integration tests using [Microsoft's in-memory test server infrastructure](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests). Follow Microsoft's documentation to set up a test project, and then write tests against your API endpoints. You will want to [substitute your Entity Framework database provider](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#customize-webapplicationfactory) with an in-memory Sqlite instance, and add a [mock authentication handler](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#mock-authentication) to simulate authentication (we're mainly focused on testing _authorization_, not _authentication_).


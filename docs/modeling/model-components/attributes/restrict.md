# [Restrict]

`IntelliTect.Coalesce.DataAnnotations.RestrictAttribute<T>`

In addition to [role-based](/modeling/model-components/attributes/security-attribute.md) property restrictions, you can also define property restrictions that can execute custom code for each model instance if your logic requires more nuanced decisions than can be made with roles.

``` c#:no-line-numbers
using IntelliTect.Coalesce.DataAnnotations;
public class Employee 
{
    public int Id { get; set; }

    [Read]
    public string UserId { get; set; }

    [Restrict<SalaryRestriction>]
    public decimal Salary { get; set; }
    
    public string Department { get; set; }
}

public class SalaryRestriction(MyUserService userService) : IPropertyRestriction<Employee>
{
    public bool UserCanRead(IMappingContext context, string propertyName, Employee model)
        => context.User.GetUserId() == model.UserId || userService.IsPayroll(context.User);

    public bool UserCanWrite(IMappingContext context, string propertyName, Employee model, object incomingValue)
        => userService.IsPayroll(context.User);

    public bool UserCanFilter(IMappingContext context, string propertyName)
        => userService.IsPayroll(context.User);
}
```

Restriction classes support dependency injection, so you can inject any supplemental services needed to make a determination.

The `UserCanRead` method controls whether values of the restricted property will be mapped from model instances to the generated DTO. Similarly, `UserCanWrite` controls whether the property can be mapped back to the model instance from the generated DTO.

The `UserCanFilter` method has a default implementation that returns `false`, but can be implemented if there is an appropriate, instance-agnostic way to determine if a user can sort, search, or filter values of that property.

Multiple different restrictions can be placed on a single property; all of them must succeed for the operation to be permitted. Restrictions also stack on top of role attribute restrictions (`[Read]` and `[Edit]`).

A non-generic variant of `IPropertyRestriction` also exists for restrictions that might be reused across multiple model types.


## Security Considerations

When implementing the logic for `UserCanWrite`, you must be aware that the `model` instance may already have some of its other property values populated with other client-provided values. If you're making security determinations based on other writable properties of that model, you need to make sure that those other properties are also properly secured.

## Performance Considerations

Restrictions are evaluated for each object they apply to. If returning large amounts of data to a client, this could result in hundreds or thousands of invocations. If the logic you are evaluating depends on external resources (i.e. your database), you will need to be careful of the impact of performing that evaluation repeatedly.

If you need to fetch an entity from the database, consider using `DbSet<>.Find` if possible, which will retrieve an existing tracked entity instance from the `DbContext` if one exists instead of querying for a fresh record from the database.

For all other cases, consider caching any data you retrieve on the `IPropertyRestriction` instance itself - for example, in a `Dictionary<,>` field. The same restriction instance is used for all objects being mapped in the current request. Restrictions are evaluated serially so concurrency issues are not a concern.

``` c#
public class SalaryRestriction(AppDbContext db) : IPropertyRestriction<Employee>
{
    private readonly Dictionary<string, bool> _departmentManagerCache = new();

    public bool UserCanRead(IMappingContext context, string propertyName, Employee model)
    {
        if (context.User.GetUserId() == model.UserId) return true;
        
        // Use Find to leverage EF tracking for better performance
        var currentUser = db.Users.Find(context.User.GetUserId());
        if (currentUser?.IsPayroll == true) return true;
        
        return IsDepartmentManager(context.User.GetUserId(), model.Department);
    }

    private bool IsDepartmentManager(string userId, string department)
    {
        // Cache department manager checks to avoid repeated database queries
        var cacheKey = $"{userId}:{department}";
        if (_departmentManagerCache.TryGetValue(cacheKey, out bool isManager))
            return isManager;
        
        return _departmentManagerCache[cacheKey] = db.DepartmentManagers
            .Any(dm => dm.UserId == userId && dm.Department == department);
    }
}
```
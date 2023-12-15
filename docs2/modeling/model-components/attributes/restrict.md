# [Restrict]

In addition to [role-based](/modeling/model-components/attributes/security-attribute.md) property restrictions, you can also define property restrictions that can execute custom code for each model instance if your logic require more nuanced decisions than can be made with roles.

``` c#:no-line-numbers
using IntelliTect.Coalesce.DataAnnotations;
public class Employee 
{
  public int Id { get; set; }

  [Read]
  public string UserId { get; set; }

  [Restrict<SalaryRestriction>]
  public decimal Salary { get; set; }
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
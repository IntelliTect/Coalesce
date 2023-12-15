
# [Inject]
========

Used to mark a [Method](/modeling/model-components/methods.md) parameter for dependency injection from the application's `IServiceProvider`.

See [Methods](/modeling/model-components/methods.md) for more.

This gets translated to a `Microsoft.AspNetCore.Mvc.FromServicesAttribute` in the generated API controller's action.


## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string GetFullName([Inject] ILogger<Person> logger)
    {
        logger.LogInformation("Person " + PersonId + "'s full name was requested");
        return FirstName + " " + LastName";
    }
}
```

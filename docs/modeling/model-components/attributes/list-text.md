
# [ListText]

When a textual representation of an object needs to be displayed in the UI, this attribute controls which property will be used. Examples include dropdowns and cells in admin UI tables.

If this attribute is not used, and a property named `Name` exists on the model, that property will be used. Otherwise, the primary key will be used.


## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    [ListText]
    [NotMapped]
    public string Name => FirstName + " " + LastName
}
```

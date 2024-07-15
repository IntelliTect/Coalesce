
# [InternalUse]

`IntelliTect.Coalesce.DataAnnotations.InternalUseAttribute`

Used to mark a type, property or method for internal use. Internal Use members are:

- Not exposed via the API.
- Not present in the generated TypeScript view models.
- Not present nor accounted for in the generated C# DTOs.
- Not present in the generated editor or list views.

Effectively, an Internal Use member is invisible to Coalesce. This attribute can be considered a [Security Attribute](/modeling/model-components/attributes/security-attribute.md).

Note that this only needs to be used on members that are public. Non-public members (including `internal`) are always invisible to Coalesce.


## Example Usage

In this example, `Color` is the property exposed to the API, but `ColorHex` is the property that maps to the database that stores the value. A helper method also exists for the color generation, but needs no attribute to be hidden since methods must be explicitly exposed with [[Coalesce]](/modeling/model-components/attributes/coalesce.md).

If no color is saved in the database (the user hasn't picked a color), one is deterministically created.

``` c#
public class ApplicationUser
{
    public int ApplicationUserId { get; set; }

    [InternalUse]
    public string ColorHex { get; set; }

    [NotMapped]
    public string Color
    {
        get => ColorHex ?? GenerateColor(ApplicationUserId).ToRGBHexString();
        set => ColorHex = value;
    }

    public static HSLColor GenerateColor(int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        return new HSLColor(random.NextDouble(), random.Next(40, 100) / 100d, random.Next(25, 65) / 100d);
    }
}
```

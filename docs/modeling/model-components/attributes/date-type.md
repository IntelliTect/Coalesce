# [DateType]

Specifies whether a DateTime type will have a date and a time, or only a date.

## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }

    [DateType(DateTypeAttribute.DateTypes.DateOnly)]
    public DateTimeOffset? BirthDate { get; set; }
}
```

## Properties

<Prop def="public DateTypes DateType { get; set; } = DateTypes.DateTime; " ctor=1 /> 

The type of date the property represents.

Enum values are:
- `DateTypeAttribute.DateTypes.DateTime` Subject is both a date and time.
- `DateTypeAttribute.DateTypes.DateOnly` Subject is only a date with no significant time component.
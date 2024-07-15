---
deprecated: true
---

# [CreateController]

By default an API and View controller are both created. This allows for
suppressing the creation of either or both of these.


## Example Usage

``` c#
[CreateController(view: false, api: true)]
public class Person
{
    public int PersonId { get; set; }
    
    ...
}
```

## Properties

<Prop def="public bool WillCreateView { get; set; } = true" ctor=1 />

<Prop def="public bool WillCreateApi { get; set; } = true" ctor=2 />

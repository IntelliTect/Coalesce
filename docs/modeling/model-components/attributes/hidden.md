
# [Hidden]

`IntelliTect.Coalesce.DataAnnotations.HiddenAttribute`

Mark an property as hidden from the edit, List or All areas.

::: danger
This attribute is **not a security attribute** - it only affects the rendering of the admin pages. It has no impact on data visibility in the API.

Do not use it to keep certain data private - use the [Security Attributes](/modeling/model-components/attributes/security-attribute.md) family of attributes for that.
:::   

## Example Usage

``` c#
public class Person
{
    public int PersonId { get; set; }

    [Hidden(HiddenAttribute.Areas.All)]
    public int? IncomeLevelId { get; set; }
}
```

## Properties
<Prop def="public Areas Area { get; set; } = Areas.All;" ctor=1 />
    
The areas in which the property should be hidden.

Enum values are:
- `HiddenAttribute.Areas.None` Hide from no generated views. Primary and Foreign keys are hidden by default - setting this value explicitly can override this default behavior.
- `HiddenAttribute.Areas.All` Hide from all generated views
- `HiddenAttribute.Areas.List` Hide from generated list views only (Knockout Table/Cards, Vue `c-admin-table`)
- `HiddenAttribute.Areas.Edit` Hide from generated editor only (Knockout CreateEdit, Vue `c-admin-editor`)


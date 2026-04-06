
# [CoalesceMetadata]

`IntelliTect.Coalesce.DataAnnotations.CoalesceMetadataAttribute`

Placed on an assembly, specifies an attribute type whose values should be extracted from any Coalesce-generated types, properties, methods, and parameters, and emitted into the generated TypeScript metadata.

The extracted attribute values are available in the `attributes` property of the corresponding metadata object in `metadata.g.ts`.

## Example Usage

Register the attribute type at the assembly level in your data project (e.g. in any `.cs` file):

```c#
[assembly: CoalesceMetadata<System.ComponentModel.CategoryAttribute>]
```

Then use the attribute on your models:

```c#
[Category("people")]
public class Person
{
    [Category("contact")]
    public string? Email { get; set; }

    [Category("contact")]
    public string? Phone { get; set; }

    [return: Category("action")]
    public ItemResult ChangeSpaces(string newSpaces) { ... }
}
```

## Properties

<Prop def="Type AttributeType" />

The attribute type whose values should be extracted and emitted into metadata. All constructor arguments and explicitly-set named properties on the attribute will be included.

<Prop def="string? Key" />

The key name to use in the generated metadata. If not specified, the attribute type name is used with the `Attribute` suffix removed and camelCased (e.g. `FeatureFlagAttribute` → `featureFlag`).

## Output Format

- **Parameterless attributes**: Emitted as an empty object `{}`.
- **All other attributes**: Emitted as an object with camelCased property names.

Supported value types include strings, numbers, booleans, enums (emitted as their string representation), and arrays of these types.

## TypeScript Type

The `attributes` property is available on all metadata interfaces:

```ts
readonly attributes?: {
  readonly [key: string]: { readonly [key: string]: unknown }
}
```

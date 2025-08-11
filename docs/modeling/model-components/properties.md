# Properties

Models in a Coalesce application are just EF Core POCOs. The properties defined on your models should fit within the constraints of EF Core.

Coalesce currently has a few more restrictions than what EF Core allows, but hopefully over time some of these restrictions can be relaxed as Coalesce grows in capability.

## Property Varieties

The following kinds of properties may be declared on your models.

### Primitives, Scalars, & Dates

Most common built-in primitive (numerics, strings, booleans) and other scalar data types (enums, [date types](/topics/working-with-dates.md), `Guid`, `Uri`), and their nullable variants, are all supported as model properties. Collections of these types are also supported.

#### Enum String Serialization

Enums can be serialized as strings in API responses by annotating them with `[JsonConverter(typeof(JsonStringEnumConverter))]`:

```c#
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Active = 1,
    Inactive = 2,
    Pending = 3
}
```

When this attribute is present, the enum values will be sent as strings (e.g., `"Active"`) in JSON responses instead of numbers. The generated TypeScript enums and ViewModels continue to use numeric values on the client-side, with transparent conversion handled automatically by Coalesce's serialization layer.

### Non-mapped POCOs

Properties of a type that are not on your `DbContext` will also have corresponding properties generated on the [TypeScript ViewModels](/stacks/vue/layers/viewmodels.md#generated-members) typed as [Plain Models](/stacks/vue/layers/models.md), and the values of such properties will be sent with the object to the client when requested. Properties of this type will also be sent back to the server by the client when they are encountered.

See [Simple Models](/modeling/model-types/simple-models.md) for more information.

### Getter-only Properties

Any property that only has a getter will also have a corresponding property generated in the [TypeScript ViewModels](/stacks/vue/layers/viewmodels.md#generated-members) and will receive values of the property from the server, but values won't be sent back to the server.

If such a property is defined as an auto-property, the `[NotMapped]` attribute should be used to prevent EF Core from attempting to map such a property to your database.

### Init-only Properties

Properties on [CRUD Models](/modeling/model-types/crud.md) that use an `init` accessor rather than a `set` accessor will be implicitly treated as required, and can also only have a value provided when the entity is created for the first time. Any values provided during save actions for init-only properties when updating an existing entity will be ignored.

### Const Fields

Const fields declared on your models, services, and data sources, if annotated with `[Coalesce]`, will be emitted into the generated TypeScript Models and ViewModels. For example, `[Coalesce] public const int MagicNumber = 42;`.

## Property Customization

For any of the kinds of properties outlined above, the following customizations can be applied:

### Attributes

Coalesce provides a number of [Attributes](/modeling/model-components/attributes.md), and supports a number of other .NET attributes, that allow for further customization of your model.

### Security

Property values received by the server from the client will be ignored if rejected by any [property-level Security](/topics/security.md#property-column-security). This security is implemented in the [Generated C# DTOs](/stacks/agnostic/dtos.md).

### Loading & Serialization

The [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior), any custom functionality defined in [Data Sources](/modeling/model-components/data-sources.md), and [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) may also restrict which properties are sent to the client when requested.

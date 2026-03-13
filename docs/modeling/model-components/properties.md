# Properties

The properties on your [CRUD Models](/modeling/model-types/crud.md) and [Simple Models](/modeling/model-types/simple-models.md) determine the shape of the generated TypeScript and API layers in Coalesce.

## Property Varieties

The following kinds of properties may be declared on your models.

### Primitives, Scalars, & Dates

Most common built-in primitive (numerics, strings, booleans) and other scalar data types (enums, [date types](/topics/working-with-dates.md), `Guid`, `Uri`), and their nullable variants, are all supported as model properties. Collections of these types are also supported.

### Object Properties

Properties whose type is another complex object (as opposed to a [primitive or scalar](#primitives-scalars-dates)) fall into two categories based on whether the property type has a `DbSet<T>` on the `DbContext`:

#### Entity Properties

Properties whose type is another [Entity Model](/modeling/model-types/entities.md) (a type with a `DbSet<T>` on your `DbContext`) represent relational navigation properties. These include foreign key / reference navigations (single objects) and collection navigations. See the [Relational Modeling](/modeling/model-types/entities.md#relational-modeling) section of the Entity Models page for details on how to configure these relationships.

#### Simple Model Properties

Properties whose type does not have a `DbSet<T>` on your `DbContext` are [Simple Model](/modeling/model-types/simple-models.md) properties. These will have corresponding properties generated on the [TypeScript ViewModels](/stacks/vue/layers/viewmodels.md#generated-members), and their values are round-tripped to and from the server in all operations. Collections of simple models are also supported.

When placed on entity models, these properties can optionally be mapped to JSON columns in EF — see [JSON-mapped Properties](/modeling/model-types/entities.md#json-mapped-properties).

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

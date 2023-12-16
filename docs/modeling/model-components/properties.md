
# Properties

Models in a Coalesce application are just EF Core POCOs. The properties defined on your models should fit within the constraints of EF Core.

Coalesce currently has a few more restrictions than what EF Core allows, but hopefully over time some of these restrictions can be relaxed as Coalesce grows in capability.

## Property Varieties

The following kinds of properties may be declared on your models.

### Primary Key
To work with Coalesce, your model must have a single property for a primary key. By convention, this property should be named the same as your model class with `Id` appended to that name, but you can also annotate a property with `[Key]` or name it exactly "Id" to denote it as the primary key.

### Foreign Keys & Reference Navigation Properties
While a foreign key may be declared on your model using only the EF OnModuleBuilding method to specify its purpose, Coalesce won't know what the property is a key for. Therefore, foreign key properties should always be accompanied by a reference navigation property, and vice versa.

In cases where the foreign key is not named after the navigation property with `"Id"` appended, the `[ForeignKeyAttribute]` may be used on either the key or the navigation property to denote the other property of the pair, in accordance with the recommendations set forth by [EF Core's Modeling Guidelines](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/mapping-attributes#foreignkeyattribute).

### Collection Navigation Properties
Collection navigation properties can be used in a straightforward manner. In the event where the inverse property on the other side of the relationship cannot be determined, `[InversePropertyAttribute]` will need to be used. [EF Core provides documentation](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/mapping-attributes#inversepropertyattribute) on how to use this attribute. Errors will be displayed at generation time if an inverse property cannot be determined without the attribute. We recommend recommended that you declare the type of collection navigation properties as `ICollection<T>`.

### Non-mapped POCOs
Properties of a type that are not on your `DbContext` will also have corresponding properties generated on the [TypeScript ViewModels](/stacks/disambiguation/view-model.md) typed as [TypeScript External ViewModels](/stacks/disambiguation/external-view-model.md), and the values of such properties will be sent with the object to the client when requested. Properties of this type will also be sent back to the server by the client when they are encountered (currently supported by the [Vue Stack](/stacks/vue/overview.md) only).

See [External Types](/modeling/model-types/external-types.md) for more information.

### Primitives, Scalars, & Dates
Most common built-in primitive and scalar data types (numerics, strings, booleans, enums, `DateTime`, `DateTimeOffset`), and their nullable variants, are all supported as model properties.

### Getter-only Properties
Any property that only has a getter will also have a corresponding property generated in the [TypeScript ViewModels](/stacks/disambiguation/view-model.md), but won't be sent back to the server during any save actions.

If such a property is defined as an auto-property, the `[NotMapped]` attribute should be used to prevent EF Core from attempting to map such a property to your database.

### Init-only Properties
Properties on [Entity Models](/modeling/model-types/entities.md) that use an `init` accessor rather than a `set` accessor will be implicitly treated as required, and can also only have a value provided when the entity is created for the first time. Any values provided during save actions for init-only properties when updating an existing entity will be ignored.



## Property Customization

For any of the kinds of properties outlined above, the following customizations can be applied:

### Attributes
Coalesce provides a number of [Attributes](/modeling/model-components/attributes.md), and supports a number of other .NET attributes, that allow for further customization of your model.

### Security
Property values received by the server from the client will be ignored if rejected by any [property-level Security](/topics/security.md#property-column-security). This security is implemented in the [Generated C# DTOs](/stacks/agnostic/dtos.md).

### Loading & Serialization
The [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior), any custom functionality defined in [Data Sources](/modeling/model-components/data-sources.md), and [[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) may also restrict which properties are sent to the client when requested.

### NotMapped
While Coalesce does not do anything special for the `[NotMapped]` attribute, it is still an important attribute to keep in mind while building your model, as it prevents EF Core from doing anything with the property.
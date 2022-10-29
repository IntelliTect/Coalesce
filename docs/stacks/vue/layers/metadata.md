# Metadata Layer

<!-- MARKER:summary -->

The metadata layer, generated as `metadata.g.ts`, contains information about the types, properties, methods, and other components of your data model. Because Vue applications are typically compiled into a set of static assets, it is necessary for the frontend code to have a representation of your data model as an analog to the `ReflectionRepository` available at runtime to Knockout apps that utilize `.cshtml` files.

<!-- MARKER:summary-end -->

[[toc]]

## Concepts 

The following is a non-exhaustive list of the general concepts used by the metadata layer. The [source code of coalesce-vue](https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/metadata.ts) provides the most exhaustive set of documentation about the metadata layer: 

### Metadata

All objects in the metadata layer that represent any kind of metadata have, at the very least, a `name`, the name of the metadata element in code (type names, property names, parameter names, etc). and a `displayName`, the human-readable form of the name that is suitable for presentation when needed. Names follow the casing convention of their corresponding language elements - types are PascalCased, while other things like properties, methods, and parameters are camelCased.

### Type

All custom types exposed by your application's data model will have a Type metadata object generated. This includes both C# classes, and C# enums. Class types include `model` (for [Entity Models](/modeling/model-types/entities.md) and [Custom DTOs](/modeling/model-types/dtos.md)) and `object` (for [External Types](/modeling/model-types/external-types.md)).

### Value

In the metadata layer, a Value is the usage of a type. This could be any type - strings, numbers, enums, classes, or even void. Values can be found in the collection of an object's properties, a method's parameters or return value, or as a data source's parameters.

All values have the following properties:

<Prop def="type: TypeDiscriminator" lang=ts />

Type could be a language primitive like `string` or `number`, a non-primitive JavaScript type (`date`, `file`), or in the case of a custom Type, the type kind of that type (`model`, `enum`, `object`). For custom types, an additional property `typeDef` will refer to the Type metadata for that type.

<Prop def="role: ValueRole" lang=ts />

Role represents what purpose the value serves in a relational model. Either `value` (the default - no relational role), `primaryKey`, `foreignKey`, `referenceNavigation`, or `collectionNavigation`.

### Property

A Property is a more refined Value that contains a number of additional fields based on the `role` of the property. k

### Domain

The type of the default export of the generated metadata. Serves as a single root from which all other metadata can be accessed. Contains fields `types`, `enums`, and `services` as organizing structures for the different kinds of custom types.
.. _VueMetadata:

Metadata Layer
==============

The metadata layer, generated as `metadata.g.ts`, contains a minimal set of metadata to represent your data model on the front-end. Because Vue applications are typically statically compiled, it is necessary for the frontend code to have a representation of your data model as an analog to the :csharp:`ReflectionRepository` available at runtime to Knockout apps that utilize `.cshtml` files.

Concepts 
--------

The following is a non-exhaustive list of the general concepts used by the metadata layer. The `source code of coalesce-vue <https://github.com/IntelliTect/Coalesce/blob/dev/src/coalesce-vue/src/metadata.ts>`_ provides the most exhausitive set of documentation about the metadata layer: 

Metadata
    All objects in the metadata layer that represent any kind of metadata have, at the very least, a :ts:`name`, the name of the metadata element in code (type names, property names, parameter names, etc). and a :ts:`displayName`, the human-readable form of the name that is suitable for presentation when needed. Names follow the casing convention of their corresponding language elements - types are PascalCased, while other things like properties, methods, and parameters are camelCased.

Type
    All custom types exposed by your application's data model will have a Type metadata object generated. This includes both C# classes, and C# enums. Class types include :ts:`model` (for :ref:`EntityModels` and :ref:`CustomDTOs`) and :ts:`object` (for :ref:`ExternalTypes`).

Value
    In the metadata layer, a Value is the usage of a type. This could be any type - strings, numbers, enums, classes, or even void. Values can be found in the collection of an object's properties, a method's parameters or return value, or as a data source's parameters.

    All values have a:

    :ts:`type`
        Type could be a language primitive like :ts:`string` or :ts:`number`, a non-primitive JavaScript type (:ts:`date`, :ts:`file`), or in the case of a custom Type, the type kind of that type (:ts:`model`, :ts:`enum`, :ts:`object`). For custom types, an additional property :ts:`typeDef` will refer to the Type metadata for that type.
    :ts:`role`
        Role represents what purpose the value serves in a relational model. Either `value` (the default - no relational role), `primaryKey`, `foreignKey`, `referenceNavigation`, or `collectionNavigation`.

Property
    A Property is a more refined Value that contains a number of additional fields based on the :ts:`role` of the property. k

Domain
    The type of the default export of the generated metadata. Serves as a single root from which all other metadata can be accessed. Contains fields :ts:`types`, :ts:`enums`, and :ts:`services` as organizing structures for the different kinds of custom types.
# Generated C# DTOs

Data Transfer Objects, or DTOs, allow for transformations of data from the data store into a format more suited for transfer and use on the client side. This often means trimming properties and flattening structures to provide a leaner over-the-wire experience. Coalesce aims to support this as seamlessly as possible.

Coalesce supports two types of DTOs:

-  DTOs that are automatically generated for each POCO database object.
   These are controlled via [Attributes](/modeling/model-components/attributes.md) on the POCO. These are outlined below.
-  DTOs that you create with IClassDto. These are outlined at [Custom DTOs](/modeling/model-types/dtos.md).

## Automatically Generated DTOs

Every class that is exposed through Coalesce's generated API will have a corresponding DTO generated for it. These DTOs are used to shuttle data back and forth to the client. They are generated classes that have nullable versions of all the properties on the POCO class.

[[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) and the [Includes String](/concepts/includes.md) infrastructure can be used to indicate which properties should be transferred to the client in which cases, and [Include Tree](/concepts/include-tree.md) is used to dictate how these DTOs are constructed from POCOs retrieved from the database.

The [[Read] and [Edit] attributes](/modeling/model-components/attributes/security-attribute.md) can be used to apply property-level security, which manifests as conditional logic in the mapping methods on the generated DTOs.

See the [Security](/topics/security.md#attributes) page to read more about property-level security, as well as all other security mechanisms in Coalesce.


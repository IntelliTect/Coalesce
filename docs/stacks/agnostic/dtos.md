# DTOs

Data Transfer Objects, or DTOs, allow for transformations of data from the data store into a format more suited for transfer and use on the client side. This often means trimming properties and flattening structures to provide a leaner over-the-wire experience. Coalesce aims to support this as seamlessly as possible.

Coalesce supports two types of DTOs:

- [Generated DTOs](#automatically-generated-dtos), created by Coalesce's code generator for each [entity model](/modeling/model-types/entities.md), [standalone entity](/modeling/model-types/standalone-entities.md), and [external type](/modeling/model-types/external-types.md).
- [Custom DTOs](/modeling/model-types/dtos.md), which are created manually by you, the developer.

## Automatically Generated DTOs

Every class that is exposed through Coalesce's generated API will have a corresponding DTO generated for it. These DTOs are used to shuttle data back and forth to the client. They are generated classes that have nullable versions of all the properties on the POCO class.


The [[Read], [Edit], and [Restrict] attributes](/modeling/model-components/attributes/security-attribute.md) can be used to apply property-level security, which manifests as conditional logic in the mapping methods on the generated DTOs. These attributes are enforced in the mapping operations to and from the generated DTO.

See the [Security](/topics/security.md#property-column-security) page to read more about property-level security, as well as all other security mechanisms in Coalesce.

[[DtoIncludes] & [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md) can be used to indicate which properties should be transferred to the client in which cases, and [Include Trees](/concepts/include-tree.md) are used to dictate how these DTOs are shaped when mapping from your domain objects. Do note that these are not security features.

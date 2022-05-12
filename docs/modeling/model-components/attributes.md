# Attributes

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties in order to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from `System.ComponentModel.DataAnnotations`.

[[toc]]

## Coalesce Attributes

Browse the list in the sidebar to learn about the attributes that Coalesce provides that can be used to decorate your models.

<!-- TODO: Is there some kind of metadata we can use to dynamically source the coalesce attribute page and list them here instead of directing the reader to the sidebar? -->

## ComponentModel Attributes

Coalesce also supports a number of the built-in `System.ComponentModel.DataAnnotations` attributes and will use these to shape the generated code.

### [Display]

The displayed name and description of a property, as well as the order in which it appears in generated views, can be set via the `[Display]` attribute. By default, properties will be displayed in the order in which they are defined in their class.

### [DisplayName]

The displayed name of a property can also be set via the `[DisplayName]` attribute.

### [Required]

Properties with `[Required]` will generate client validation rules. See [[ClientValidation]](/modeling/model-components/attributes/client-validation.md).

### [Range]

Properties with `[Range]` will generate client validation rules. See [[ClientValidation]](/modeling/model-components/attributes/client-validation.md).

### [MinLength]

Properties with `[MinLength]` will generate client validation rules. See [[ClientValidation]](/modeling/model-components/attributes/client-validation.md).

### [MaxLength]

Properties with `[MaxLength]` will generate client validation rules. See [[ClientValidation]](/modeling/model-components/attributes/client-validation.md).


### [ForeignKey]

Normally, Coalesce figures out which properties are foreign keys, but if you don't use standard EF naming conventions then you'll need to annotate with `[ForeignKey]` to help out both EF and Coalesce. See the [Entity Framework Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships) documentation for more.

### [InverseProperty]

Sometimes, Coalesce (and EF, too) can have trouble figuring out what the foreign key is supposed to be for a collection navigation property. See the [Entity Framework Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships) documentation for details on how and why to use `[InverseProperty]`.

### [DatabaseGenerated]

Primary Keys with `[DatabaseGenerated(DatabaseGeneratedOption.None)]` will be settable on the client and will be appropriately handled by the [Standard Behaviors](/modeling/model-components/behaviors.md#standard-behaviors) on the server. Currently unsupported on the [Knockout front-end stack](/stacks/ko/overview.md).

### [NotMapped]

Model properties that aren't mapped to the database should be marked with `[NotMapped]` so that Coalesce doesn't try to load them from the database when [searching](/modeling/model-components/attributes/search.md) or carrying out the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior).
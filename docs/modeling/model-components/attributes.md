# Attributes

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from `System.ComponentModel.DataAnnotations`.

## Coalesce Attributes

Browse the list in the sidebar to learn about the attributes that Coalesce provides for decorating your models.

<!-- TODO: Is there some kind of metadata we can use to dynamically source the coalesce attribute page and list them here instead of directing the reader to the sidebar? -->

## ComponentModel Attributes

Coalesce also supports a number of the built-in `System.ComponentModel.DataAnnotations` attributes and will use these to shape the generated code.

### [Display]

`System.ComponentModel.DataAnnotations.DisplayAttribute`

The displayed name and description of a property, as well as the order in which it appears in generated views, can be set via the `[Display]` attribute. By default, properties will be displayed in the order in which they are defined in their class.

### [DisplayName]

`System.ComponentModel.DisplayNameAttribute`

The displayed name of a property can also be set via the `[DisplayName]` attribute.

### [Description]

`System.ComponentModel.DescriptionAttribute`

The description of a type or member, such as a class, property, method, or parameter.

### [Required]

`System.ComponentModel.DataAnnotations.RequiredAttribute`

Properties with `[Required]` will generate [client validation](/modeling/model-components/attributes/client-validation.md) and [server validation](/topics/security.md#server-side-data-validation) rules.

### [Range]

`System.ComponentModel.DataAnnotations.RangeAttribute`

Properties with `[Range]` will generate [client validation](/modeling/model-components/attributes/client-validation.md) and [server validation](/topics/security.md#server-side-data-validation) rules.

### [MinLength]

`System.ComponentModel.DataAnnotations.MinLengthAttribute`

Properties with `[MinLength]` will generate [client validation](/modeling/model-components/attributes/client-validation.md) and [server validation](/topics/security.md#server-side-data-validation) rules.

### [MaxLength]

`System.ComponentModel.DataAnnotations.MaxLengthAttribute`

Properties with `[MaxLength]` will generate [client validation](/modeling/model-components/attributes/client-validation.md) and [server validation](/topics/security.md#server-side-data-validation) rules.

### [DataType]

`System.ComponentModel.DataAnnotations.DataTypeAttribute`

Some values of `DataType` when provided to `DataTypeAttribute` on a `string` property will alter the behavior of the [Vue Components](/stacks/vue/coalesce-vue-vuetify/overview.md). See [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display.md) and [c-input](/stacks/vue/coalesce-vue-vuetify/components/c-input.md) for details.

### [ForeignKey]

`System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute`

Normally, Coalesce figures out which properties are foreign keys, but if you don't use standard EF naming conventions then you'll need to annotate with `[ForeignKey]` to help out both EF and Coalesce. See the [Entity Framework Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships) documentation for more.

### [InverseProperty]

`System.ComponentModel.DataAnnotations.Schema.InversePropertyAttribute`

Sometimes, Coalesce (and EF, too) can have trouble figuring out what the foreign key is supposed to be for a collection navigation property. See the [Entity Framework Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships) documentation for details on how and why to use `[InverseProperty]`.

### [DatabaseGenerated]

`System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute`

Primary Keys with `[DatabaseGenerated(DatabaseGeneratedOption.None)]` will be settable on the client and will be appropriately handled by the [Standard Behaviors](/modeling/model-components/behaviors.md#standard-behaviors) on the server. 

### [NotMapped]

`System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute`

Model properties that aren't mapped to the database should be marked with `[NotMapped]` so that Coalesce doesn't try to load them from the database when [searching](/modeling/model-components/attributes/search.md) or carrying out the [Default Loading Behavior](/modeling/model-components/data-sources.md#default-loading-behavior).

### [DefaultValue]

`System.ComponentModel.DefaultValueAttribute`

Properties with `[DefaultValue]` will receive the specified value when a new ViewModel is instantiated on the client. This enables scenarios like pre-filling a required property with a suggested value.

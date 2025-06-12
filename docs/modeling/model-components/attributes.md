# Attributes

Coalesce provides a number of C# attributes that can be used to decorate your model classes and their properties in order to customize behavior, appearance, security, and more. Coalesce also supports a number of annotations from `System.ComponentModel.DataAnnotations`.

## Coalesce Attributes

Coalesce provides the following attributes that can be used to decorate your models, properties, and methods:

### Core Attributes
- **[[Coalesce]](/modeling/model-components/attributes/coalesce.md)** - Marks classes, properties, and methods for inclusion in code generation
- **[[InternalUse]](/modeling/model-components/attributes/internal-use.md)** - Excludes items from client-side generation while keeping them available server-side

### Security Attributes  
- **[[Read], [Edit], [Create], [Delete]](/modeling/model-components/attributes/security-attribute.md)** - Control access permissions for operations
- **[[Execute]](/modeling/model-components/attributes/execute.md)** - Specify required roles for method execution
- **[[Restrict]](/modeling/model-components/attributes/restrict.md)** - Apply additional restrictions to operations

### Display & Behavior Attributes
- **[[Hidden]](/modeling/model-components/attributes/hidden.md)** - Hide properties from generated admin interfaces
- **[[Search]](/modeling/model-components/attributes/search.md)** - Configure search behavior for properties
- **[[ListText]](/modeling/model-components/attributes/list-text.md)** - Specify the display text for objects in lists and dropdowns
- **[[DefaultOrderBy]](/modeling/model-components/attributes/default-order-by.md)** - Set default sorting for collections

### Data Handling Attributes
- **[[DateType]](/modeling/model-components/attributes/date-type.md)** - Specify how date/time properties should be handled
- **[[ManyToMany]](/modeling/model-components/attributes/many-to-many.md)** - Configure many-to-many relationships
- **[[DtoIncludes], [DtoExcludes]](/modeling/model-components/attributes/dto-includes-excludes.md)** - Control what gets included in DTOs

### Advanced Attributes
- **[[Inject]](/modeling/model-components/attributes/inject.md)** - Inject services into method parameters
- **[[LoadFromDataSource]](/modeling/model-components/attributes/load-from-data-source.md)** - Specify custom data source for loading entities
- **[[ClientValidation]](/modeling/model-components/attributes/client-validation.md)** - Add custom client-side validation
- **[[Controller]](/modeling/model-components/attributes/controller.md)** - Customize controller generation
- **[[ControllerAction]](/modeling/model-components/attributes/controller-action.md)** - Customize individual controller actions  
- **[[CreateController]](/modeling/model-components/attributes/create-controller.md)** - Control whether to generate controllers

Browse the individual attribute pages in the sidebar for detailed information and examples.

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

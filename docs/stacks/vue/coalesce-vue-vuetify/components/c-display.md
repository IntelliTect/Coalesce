
# c-display

<!-- MARKER:summary -->
A general-purpose component for displaying any [Value](/stacks/vue/layers/metadata.md#value) by rendering the value to a string with the [display functions from the Models Layer](/stacks/vue/layers/models.md#vuemodeldisplayfunctions). For plain string and number [values](/stacks/vue/layers/metadata.md), usage of this component is largely superfluous. For all other value types including dates, booleans, enums, objects, and collections, it is very handy.
<!-- MARKER:summary-end -->


## Examples

Typical usage, providing an object and a property on that object:

``` vue-html
<c-display :model="person" for="gender" />
```

Customizing date formatting ([view format patterns](https://date-fns.org/v2.29.3/docs/format)):

``` vue-html
<c-display :model="person" for="birthDate" format="M/d/yyyy" />
```

A contrived example of using c-display to render the result of an [API Caller](/stacks/vue/layers/api-clients.md#api-callers):

``` vue-html
<c-display 
    :value="person.setFirstName.result" 
    :for="person.$metadata.methods.setFirstName.return" 
    element="div"
/>
```

Displaying a standalone date value without a model or other source of metadata:

``` vue-html
<c-display :value="dateProp" format="M/d/yyyy" />
```

## Props

<Prop def="for: string | Property | Value" lang="ts" />

A metadata specifier for the value being bound. Either a direct reference to the metadata object, or a string with the name of the value belonging to `model`, or a string in dot-notation that starts with a type name.

<Prop def="model?: Model | DataSource" lang="ts" />

An object owning the value that was specified by the `for` prop.

<Prop def="format: DisplayOptions[&quot;format&quot;]" lang="ts" />

Shorthand for `:options="{ format: format }"`, allowing for specification of the format to be used when displaying dates.

See [DisplayOptions](/stacks/vue/layers/models.md#displayoptions) for details on the options available for `format`.

<Prop def="options: DisplayOptions" lang="ts" />

Specify options for formatting some kinds of values, including dates. See [DisplayOptions](/stacks/vue/layers/models.md#displayoptions) for details.

<Prop def="modelValue: any" lang="ts" />

Can be provided the value to be displayed in conjunction with the `for` prop, as an alternative to the `model` prop.

This is an uncommon scenario - it is generally easier to use the `for`/`model` props together.

## Slots

``default`` - Used to display fallback content if the value being displayed is either `null` or `""` (empty string).


## [DataTypeAttribute]

For properties and other values annotated with [DataTypeAttribute], the following special handling occurs based on the data type:

* `DataType.MultilineText`: Renders with `white-space: pre-wrap`.
* `DataType.Password`: Renders with a show/hide toggle (hidden by default), showing a fixed number of dot characters when hidden.
* `DataType.Url`: Renders as a clickable link.
* `DataType.EmailAddress`: Renders as a clickable `mailto` link.
* `DataType.PhoneNumber`: Renders as a clickable `tel` link.
* `DataType.ImageUrl`: Renders as an `img` element.
* `"Color"`: Renders a colored dot next to the value, interpreting the field value as a 7-character HTML hex color code.


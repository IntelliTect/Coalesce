# c-datetime-picker

<!-- MARKER:summary -->
    
A general, all-purpose date/time input component that can be used either with [models](/stacks/vue/layers/models.md) and [metadata](/stacks/vue/layers/metadata.md) or as a standalone component using only ``v-model``.

<!-- MARKER:summary-end -->

[[toc]]


## Examples

``` vue-html
<c-datetime-picker :model="person" for="birthDate" />

<c-datetime-picker v-model="standaloneDate" />

<c-datetime-picker 
    v-model="standaloneTime" 
    date-kind="time"
    date-format="h:mm a"
/>
```

## Props

<Prop def="for?: string | DateProperty | DateValue" lang="ts" />

A metadata specifier for the value being bound. One of:
    
- A string with the name of the value belonging to `model`. 
- A direct reference to a metadata object.
- A string in dot-notation that starts with a type name.

<Prop def="model?: Model | DataSource" lang="ts" />

An object owning the value that was specified by the `for` prop. If provided, the input will be bound to the corresponding property on the `model` object.

<Prop def="value?: Date" lang="ts" />

If binding the component with ``v-model``, accepts the ``value`` part of ``v-model``.

<Prop def="dateKind?: 'date' | 'time' | 'datetime' = 'datetime'" lang="ts" />

Whether the date is only a date, only a time, or contains significant date `and` time information.

If the component was bound with metadata using the `for` prop, this will default to the kind specified by [[DateType]](/modeling/model-components/attributes/date-type.md).

<Prop def="dateFormat?: string" lang="ts" />

The format of the date that will be rendered in the component's text field, and the format that will be attempted first when parsing user input in the text field.

Defaults to:

- ``M/d/yyyy h:mm a`` if `dateKind == 'datetime'`, 
- ``M/d/yyyy`` if `dateKind == 'date'`, or 
- ``h:mm a`` if `dateKind == 'time'`.

::: warning
When parsing a user's text input into the text field, c-datetime-picker will first attempt to parse it with the format specified by `dateFormat`, or the default as described above if not explicitly specified.

If this fails, the date will be parsed with the [Date constructor](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/Date), but only if the `dateKind` is ``datetime`` or ``date``. This works fairly well on all modern browsers, but can still occasionally have issues. c-datetime-picker tries its best to filter out bad parses from the Date constructor, like dates with a year earlier than 1000.
:::

<Prop def="native?: boolean" lang="ts" />

True if a native HTML5 input should be used instead of a popup menu with Vuetify date/time pickers inside of it.

<Prop def="readonly?: boolean" lang="ts" />

True if the component should be read-only.

<Prop def="disabled?: boolean" lang="ts" />

True if the component should be disabled.

## Slots


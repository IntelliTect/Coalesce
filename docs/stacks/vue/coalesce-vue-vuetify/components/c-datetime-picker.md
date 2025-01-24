# c-datetime-picker

<!-- MARKER:summary -->
    
A general, all-purpose date/time input component that can be used either with [models](/stacks/vue/layers/models.md) and [metadata](/stacks/vue/layers/metadata.md) or as a standalone component using only ``v-model``.

<!-- MARKER:summary-end -->


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

<Prop def="modelValue?: Date" lang="ts" />

If binding the component with ``v-model``, accepts the ``modelValue`` part of ``v-model``.

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

If this fails, Coalesce then tries a [large number of common formats](https://github.com/IntelliTect/Coalesce/blob/1fb00c7de5e363aaf3c1a78f45af3b949b11dff4/src/coalesce-vue/test/utils.spec.ts#L5).

If that failed, then finally the date will be parsed with the [Date constructor](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/Date), but only if the `dateKind` is ``datetime`` or ``date``. This works fairly well on all modern browsers, but can still occasionally have issues. c-datetime-picker tries its best to filter out bad parses from the Date constructor, like dates with a year earlier than 1000.
:::

<Prop def="step?: number" lang="ts" />

The increments, in minutes, of the selectable value. Values should divide 60 evenly, or be multiples of 60. For example, a step of 15 allows selection of :00, :15, :30, and :45 minute time values.

<Prop def="min?: Date" lang="ts" />

The smallest allowable date/time selection.

<Prop def="max?: Date" lang="ts" />

The largest allowable date/time selection.

<Prop def="allowedDates?: Date[] | ((date: Date) => boolean)" lang="ts" />

An array of permitted dates (items should have a time of midnight),
or a function that returns true if a date is allowed for selection.
Does not impact time selection.

<Prop def="timeZone?: string" lang="ts" />

The [IANA Time Zone Database](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) name that the user will pick the date/time value in.
Defaults to the value configured with [`setDefaultTimeZone`](/stacks/vue/layers/models.md#member-setdefaulttimezone) if the value bound to with `model`/`for` is a `DateTimeOffset`.

<Prop def="native?: boolean" lang="ts" />

True if a native HTML5 input should be used instead of a popup menu with date/time pickers inside of it.

<Prop def="readonly?: boolean" lang="ts" />

True if the component should be read-only. This state is also inherited from any wrapping `v-form`.

<Prop def="disabled?: boolean" lang="ts" />

True if the component should be disabled. This state is also inherited from any wrapping `v-form`.



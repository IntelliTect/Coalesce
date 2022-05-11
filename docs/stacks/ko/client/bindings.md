
# Knockout Bindings

Coalesce provides a number of knockout bindings that make common model binding activities much easier. 

Editors Note: On this page, some bindings are split into their requisite HTML component with their ``data-bind`` component listed immediately after. Keep this in mind when reading.


[[toc]]


## Input Bindings

### select2Ajax

``` html
<select data-bind="
    select2Ajax: personId, 
    url: '/api/Person/list', 
    idField: 'personId', 
    textField: 'Name', 
    object: person, 
    allowClear: true
"></select>
```

Creates a select2 dropdown using the specified url and fields that can be used to select an object from the endpoint specified. Additional complimentary bindings include:

<Prop def="url: string" lang="ts" />

The Coalesce List API url to call to populate the contents of the dropdown.


<Prop def="idField: string" lang="ts" />

The name of the field on each item in the results of the AJAX call which contains the ID of the option. The value of this field will be set on the observable specified for the main ``select2Ajax`` binding.


<Prop def="textField: string" lang="ts" />

The name of the field on each item in the results of the AJAX call which contains the text to be displayed for each option.


<Prop def="object?: KnockoutObservable<Coalesce.BaseViewModel | null>" lang="ts" />

An observable that holds the full object corresponding to the foreign key property being bound to. If the selected value changes, this will be set to null to avoid representation of incorrect data (unless `setObject` is used - see below).


<Prop def="setObject: boolean = false" lang="ts" />

If true, the observable specified by the `object` binding will be set to the selected data when an option is chosen in the dropdown. Binding `itemViewModel` is required if this binding is set.

Additionally, requests to the API to populate the dropdown will request the entire object, as opposed to only the two fields specified for `idField` and `textField` like is normally done when this binding is missing or set to false. To override this behavior and continue requesting only specific fields even when `setObject` is true, add `fields=field1,field2,...` to the query string of the `url` binding.


<Prop def="itemViewModel?: (new (newItem: object) => Coalesce.BaseViewModel)" lang="ts" />

A reference to the class that represents the type of the object held in the `object` observable. This is used when constructing new objects from the results of the API call. Not used if `setObject` is false or unspecified. For example, `setObject: true, itemViewModel: ViewModels.Person`.


<!-- MARKER:select-params -->

<Prop def="pageSize: number = 25" lang="ts" />

The number of items to request in each call to the server.

<Prop def="format: string = '{0}'" lang="ts" />

A string containing the substring `{0}`, which will be replaced with the text value of an option in the dropdown list when the option is displayed.


<Prop def="selectionFormat: string = '{0}'" lang="ts" />

A string containing the substring `{0}`, which will be replaced with the text value of the selected option of the dropdown list.

<Prop def="cache: boolean = true" lang="ts" />

If true, a cache-busting querystring parameter will be included in AJAX requests.

<!-- MARKER:select2-params -->

<Prop def="selectOnClose: boolean = false" lang="ts" />

Directly maps to select2 option `selectOnClose`.
   

<Prop def="allowClear: boolean = true" lang="ts" />

Whether or not to allow the current select to be set to null. Directly maps to select2 option `allowClear`.
    

<Prop def="placeholder: string = 'select'" lang="ts" />

Placeholder when nothing is selected. Directly maps to select2 option `placeholder`.


<Prop def="openOnFocus: boolean = false" lang="ts" />

If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.
    

<!-- MARKER:end-select-params -->
        

### select2AjaxMultiple

``` html
<select multiple="multiple" data-bind="
    select2AjaxMultiple: people, 
    url: '/api/Person/list', 
    idField: 'personId', 
    textField: 'Name', 
    itemViewModel: ViewModels.PersonCase
"></select>
```

Creates a select2 multi-select input for choosing objects that participate as the foreign object in a many-to-many relationship with the current object. The primary `select2AjaxMultiple` binding takes the collection of items that make up the foreign side of the relationship. This is NOT the collection of the join objects (a.k.a. middle table objects) in the relationship.

Additional complimentary bindings include:

<Prop def="url: string" lang="ts" />

The Coalesce List API url to call to populate the contents of the dropdown. In order to only receive specific fields from the server, add ``fields=field1,field2,...`` to the query string of the url, ensuring that at least the ``idField`` and ``textField`` are included in that collection.

<Prop def="idField: string" lang="ts" />

The name of the field on each item in the results of the AJAX call which contains the ID of the option. The value of this field will be set as the key of the foreign object in the many-to-many relationship.


<Prop def="textField: string" lang="ts" />

The name of the field on each item in the results of the AJAX call which contains the text to be displayed for each option.

<Prop def="itemViewModel: (new (newItem: object) => Coalesce.BaseViewModel)" lang="ts" />

A reference to the class that represents the types in the supplied collection. For example, a many-to-many between ``Person`` and ``Case`` objects where ``Case`` is the object being bound to and ``Person`` is the type represented by a child collection, the correct value is  ``ViewModels.Person``. This is used when constructing new objects representing the relationship when a new item is selected.

@[import-md "after":"MARKER:select-params", "before":"MARKER:end-select-params"](./bindings.md) 


### select2AjaxText

``` html
<select data-bind="
    select2AjaxText: schoolName, 
    url: '/api/Person/SchoolNames'
"></select>
```

Creates a select2 dropdown against the specified url where the url returns a collection of string values that are potential selection candidates. The dropdown also allows the user to input any value they choose - the API simply serves suggested values.


<Prop def="url: string" lang="ts" />

The url to call to populate the contents of the dropdown. This should be an endpoint that returns one of the following:

- A raw `string[]`
- An object that conforms to `{ list: string[] }`
- An object that conforms to `{ object: string[] }`
- An object that conforms to `{ list: { [prop: string]: string } }` where the value given to ``resultField`` is a valid property of the returned objects.
- An object that conforms to `{ object: { [prop: string]: string } }` where the value given to ``resultField`` is a valid property of the returned objects.

The url will also be passed a ``search`` parameter and a ``page`` parameter appended to the query string. The chosen endpoint is responsible for implementing this functionality. Page size is expected to be some fixed value. Implementer should anticipate that the requested page may be out of range.

The cases listed above that accept arrays of objects (as opposed to arrays of strings) require that the ``resultField`` binding is also used. These are designed for obtaining string values from objects obtained from the standard ``list`` endpoint.


<Prop def="resultField?: string" lang="ts" />

If provided, specifies a field on the objects returned from the API to pull the string values from. See examples in ``url`` above.

<Prop def="allowCustom: boolean = true" lang="ts" />

If ``false``, the user's search input will not be presented as a valid selectable value; only the exact values obtained from the API endpoint will be selectable.

@[import-md "after":"MARKER:select2-params", "before":"MARKER:end-select-params"](./bindings.md) 

### select2

``` html
<select data-bind="select2: selectedNumber">
    <option value="1">Option 1</option>
    <option value="2">Option 2</option>
</select>
```

Sets up a basic select2 dropdown on an HTML select element. Dropdown contents should be populated through other means - either using stock [Knockout](http://knockoutjs.com/) bindings or server-side static contents (via cshtml).

@[import-md "after":"MARKER:select2-params", "before":"MARKER:end-select-params"](./bindings.md) 

### datePicker

``` html
<div class="input-group date">
    <input data-bind="datePicker: birthDate" type="text" class="form-control" />
    <span class="input-group-addon">
        <span class="fa fa-calendar"></span>
    </span>
</div>
```

Creates a date/time picker for changing a `moment.Moment` property. The control used is [bootstrap-datetimepicker](https://www.npmjs.com/package/eonasdan-bootstrap-datetimepicker)


<Prop def="preserveDate: boolean = false" lang="ts" />

If true, the date portion of the `moment.Moment` object will be preserved by the date picker. Only the time portion will be changed by user input.


<Prop def="preserveTime: boolean = false" lang="ts" />

If true, the time portion of the `moment.Moment` object will be preserved by the date picker. Only the date portion will be changed by user input.


<Prop def="format: string = 'M/D/YY h:mm a'" lang="ts" />

Specify the moment-compatible format string to be used as the display format for the text value shown on the date picker. Defaults to ``M/D/YY h:mm a``. Direct pass-through to [bootstrap-datetimepicker](https://www.npmjs.com/package/eonasdan-bootstrap-datetimepicker).


<Prop def="sideBySide: boolean = false" lang="ts" />

If true, places the time picker next to the date picker, visible at the same time. Direct pass-through to corresponding [bootstrap-datetimepicker](https://www.npmjs.com/package/eonasdan-bootstrap-datetimepicker) option.


<Prop def="stepping: number = 1" lang="ts" />

Direct pass-through to corresponding [bootstrap-datetimepicker](https://www.npmjs.com/package/eonasdan-bootstrap-datetimepicker) option.


<Prop def="timeZone: string = ''" lang="ts" />

Direct pass-through to corresponding [bootstrap-datetimepicker](https://www.npmjs.com/package/eonasdan-bootstrap-datetimepicker) option.


<Prop def="keyBinds = { left: null, right: null, delete: null }" lang="ts" />

Override key bindings of the date picker. Direct pass-through to corresponding [bootstrap-datetimepicker](https://www.npmjs.com/package/eonasdan-bootstrap-datetimepicker) option. 


<Prop def="updateImmediate: boolean = false" lang="ts" />

If true, the datePicker will update the underlying observable on each input change. Otherwise, the observable will only be changed when the datePicker loses focus (on `blur`).


### saveImmediately

``` html
<div data-bind="with: product">
    <input type="text" data-bind="textValue: description, saveImmediately: true" />
</div>
```

When used in a context where `$data` is a `Coalesce.BaseViewModel`, that object's `saveTimeoutMs` configuration property (see [ViewModel Configuration](/stacks/ko/client/model-config.md)) will be set to `0` when the element it is placed on gains focus. This value will be reverted to its previous value when the element loses focus. This will cause any changes to the object, including any observable bound as input on the element, to trigger a save immediately rather than after a delay (defaults to 500ms). 

### delaySave

``` html
<div data-bind="with: product">
    <input type="text" data-bind="textValue: description, delaySave: true" />
</div>
```

When used in a context where `$data` is a `Coalesce.BaseViewModel`, that object's `autoSaveEnabled` configuration property (see [ViewModel Configuration](/stacks/ko/client/model-config.md)) will be set to `false` when the element it is placed on gains focus. This will cause any changes to the object, including any observable bound as input on the element, to not trigger auto saves while the element has focus. When the element loses focus, the `autoSaveEnabled` flag will be reverted to its previous value and an attempt will be made to save the object. 
    

## Display Bindings

### tooltip
``` html
<div data-bind="tooltip: tooltipText">Some Element</div>
<div data-bind="tooltip: {title: note, placement: 'bottom', animation: false}">Some Element</div>
```

Wrapper around the [Bootstrap tooltip component](https://getbootstrap.com/docs/3.3/javascript/#tooltips). Binding can either be simply a string (or observable string), or it can be an object that will be passed directly to the Bootstrap tooltip component.

### fadeVisible
``` html
<div data-bind="fadeVisible: isVisible">Some Element</div>
```

Similar to the Knockout `visible` binding, but uses jQuery `fadeIn/fadeOut` calls to perform the transition.

### slideVisible
``` html
<div data-bind="slideVisible: isVisible">Some Element</div>
```

Similar to the Knockout `visible`, but uses jQuery `slideIn/slideOut` calls to perform the transition.

### moment
``` html
<div data-bind="moment: momentObservable"></div>
<div data-bind="moment: momentObservable, format: 'MM/DD/YYYY hh:mm a'"></div>
```

Controls the text of the element by calling the `format` method on a moment object. 

### momentFromNow
``` html
<div data-bind="momentFromNow: momentObservable"></div>
<div data-bind="momentFromNow: momentObservable, shorten: true"></div>
```

Controls the text of the element by calling the `fromNow` method on a moment object. If shorten is true, certain phrases will be slightly shortened. 



## Utility Bindings

### let
``` html
<div class="item">
    <!-- ko let: { showControls: $data.isEditing() || $parent.editingChildren() } -->
    <button data-bind="click: $root.editItem, visible: showControls">Edit</button>
    <span data-bind="text: name"></span>
    <button data-bind="click: $root.deleteItem, visible: showControls">Delete</button>
    <!-- /ko -->
</div>
```

The let binding is a somewhat common construct used in Knockout applications, but isn't part of Knockout itself. It effectively allows the creation of variables in the binding context, allowing complex statements which may be used multiple times to be aliased for both clarity of code and better performance.


## Knockout Binding Defaults

### Knockout Helpers

These are static properties on `IntelliTect.Coalesce.Knockout.Helpers.Knockout` you can assign to somewhere in the app lifecycle startup to change the default markup generated server-side when using ``@Knockout.*`` methods to render Knockout bindings in your ``.cshtml`` files.

<Prop def="public static int DefaultLabelCols { get; set; } = 3;" />

The default number of Bootstrap grid columns a field label should span across.


<Prop def="public static int DefaultInputCols { get; set; } = 9;" />

The default number of Bootstrap grid columns a form input should span across.


<Prop def="public static string DefaultDateFormat { get; set; } = &quot;M/D/YYYY&quot;;" />

Sets the default date-only format to be used by all date/time pickers.  This only applies to models with a date-only [[DateType]](/modeling/model-components/attributes/date-type.md) attribute.


<Prop def="public static string DefaultTimeFormat { get; set; } = &quot;h:mm a&quot;;" />

Sets the default time-only format to be used by all date/time pickers.  This only applies to models with a time-only [[DateType]](/modeling/model-components/attributes/date-type.md) attribute.

<Prop def="public static string DefaultDateTimeFormat { get; set; } = &quot;M/D/YYYY h:mm a&quot;;" />
    
Sets the default date/time format to be used by all date/time pickers.  This only applies to ``DateTimeOffset`` model properties that do not have a limiting [[DateType]](/modeling/model-components/attributes/date-type.md) attribute.

::: tip Note
``DefaultDateFormat``, ``DefaultTimeFormat`` and ``DefaultDateTimeFormat`` all take various formatting strings from the Moment.js library.  A full listing can be found on the [Moment website](https://momentjs.com/docs/#/displaying/format/).
:::

### Timezone 

The date/time picker properties can be coupled with ``DateTimeOffset`` model properties to display time values localized for the current user's locale.  If you want to make the localization static, simply include a script block in your ``_Layout.cshtml`` or in a specific view that sets the default for Moment.js:

``` html
<script>
moment.tz.setDefault("America/Chicago");
</script>
```

::: tip Note
This needs to happen *after* Moment is loaded, but *before* the bootstrap-datetimepicker script is loaded.
:::

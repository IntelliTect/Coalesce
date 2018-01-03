

.. _KnockoutBindings:

Knockout Bindings
=================


Coalesce provides a number of knockout bindings that make common model binding activities much easier. 

Editors Note: On this page, some bindings are split into their requisite HTML component with their ``data-bind`` component listed immediately after. Keep this in mind when reading.


.. contents:: Contents
    :local:


Input Bindings
--------------

select2Ajax
...........

| :html:`<select data-bind="..."></select>`
| :ts:`select2Ajax: personId, url: '/api/Person/list', idField: 'personId', textField: 'Name', object: person, allowClear: true`
|

    Creates a select2 dropdown using the specified url and fields that can be used to select an object from the endpoint specified. Additional complimentary bindings include:

    idField
        The name of the field on each item in the results of the AJAX call which contains the ID of the option. The value of this field will be set on the observable specified for the main ``select2Ajax`` binding.

    textField
        The name of the field on each item in the results of the AJAX call which contain the text to be displayed for each option.

    url
        The Coalesce List API url to call to populate the contents of the dropdown.

    pageSize
        The number of items to request in each call to the server.

    format
        A string containing the substring ``{0}``, which will be replaced with the text value of an option in the dropdown list when the option is displayed.
    
    selectionFormat
        A string containing the substring ``{0}``, which will be replaced with the text value of the selected option of the dropdown list.

    object
        An observable that holds the full object that corresponds to the foreign key property being bound to. If the selected value changes, this will be set to null to avoid representation of incorrect data (unless ``setObject`` is used - see below).

    setObject
        If true, the observable specified by the :ts:`object` binding will be set to the selected data when an option is chosen in the dropdown. 

        .. danger:: 

            This WILL NOT set the :ts:`object` observable to an instance of the :ref:`TypeScriptViewModel` being selected - it will set :ts:`object` to the raw data returned from the AJAX call. This violates the type contract defined by the generated TypeScript. Use only in situations where this effect is acceptable. This behavior may change in the future.

    selectOnClose
        Directly maps to select2 option ``selectOnClose``
        
    allowClear
        Whether or not to allow the current select to be set to null. Directly maps to select2 option ``allowClear``
        
    placeholder
        Placeholder when nothing is selected. Directly maps to select2 option ``placeholder``

    openOnFocus
        If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.
        
    cache
        Controls caching behavior of the AJAX request. Defaults to false. Seems to only affect IE - Chrome will never cache JSON ajax requests.

        

select2AjaxMultiple
...................

| :html:`<select multiple="multiple" data-bind="..."></select>`
| :ts:`select2AjaxMultiple: people, url: '/api/Person/list', idField: 'personId', textField: 'Name', itemViewModel: ViewModels.PersonCase`
|

    Creates a select2 multiselect input for choosing objects that participate as the foreign object in a many-to-many relationship with the current object. The primary ``select2AjaxMultiple`` binding takes the collection of items that make up the foreign side of the relationship. This is NOT the collection of the join objects (a.k.a. middle table objects) in the relationship.

    Additional complimentary bindings include:

    idField (required)
        The name of the field on each item in the results of the AJAX call which contains the ID of the option. The value of this field will be set as the key of the foreign object in the many-to-many relationship.

    textField (required)
        The name of the field on each item in the results of the AJAX call which contains the text to be displayed for each option.

    url (required)
        The Coalesce List API url to call to populate the contents of the dropdown.

    itemViewModel (required)
        A reference to the class that represents the types in the supplied collection. For example, a many-to-many between ``Person`` and ``Case`` objects where ``Case`` is the object being bound to and ``Person`` is the type represented by a child collection, the correct value is  :ts:``ViewModels.Person``. This is used when constructing new objects representing the relationship when a new item is selected.

    pageSize
        The number of items to request in each call to the server.

    format
        A string containing the substring ``{0}``, which will be replaced with the text value of an option in the dropdown list when the option is displayed.
    
    selectionFormat
        A string containing the substring ``{0}``, which will be replaced with the text value of the selected option of the dropdown list.

    selectOnClose
        Directly maps to select2 option ``selectOnClose``
        
    allowClear
        Whether or not to allow the current select to be set to null. Directly maps to select2 option ``allowClear``
        
    placeholder
        Placeholder when nothing is selected. Directly maps to select2 option ``placeholder``

    openOnFocus
        If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.

    cache
        Controls caching behavior of the AJAX request. Defaults to false. Seems to only affect IE - Chrome will never cache JSON ajax requests.


select2AjaxText
...............

| :html:`<select data-bind="..."></select>`
| :ts:`select2AjaxText: schoolName, url: '/api/Person/SchoolNames'`
|

    Creates a select2 dropdown against the specified url where the url returns a collection of string values that are potential selection candidates. The dropdown also allows the user to input any value they choose - the API simply serves suggested values.

    url
        The url to call to populate the contents of the dropdown. This should be an endpoint that returns one of the following:

            - A raw :ts:`string[]`
            - An object that conforms to :ts:`{ list: string[] }`
            - An object that conforms to  :ts:`{ object: string[] }`

        The url will also be passed a ``search`` parameter and a ``page`` parameter appended to the query string. The chosen endpoint is responsible for implementing this functionality. Page size is expected to be some fixed value. Implementer should anticipate that the requested page may be out of range.

    selectOnClose
        Directly maps to select2 option ``selectOnClose``

    openOnFocus
        If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.
    
    allowClear
        Whether or not to allow the current select to be set to null. Directly maps to select2 option ``allowClear``
    
    placeholder
        Placeholder when nothing is selected. Directly maps to select2 option ``placeholder``
    
    cache
        Controls caching behavior of the AJAX request. Defaults to false. Seems to only affect IE - Chrome will never cache JSON ajax requests.


select2
.......

| :html:`<select data-bind="..."></select>`
| :ts:`select2: personId`
|

    Sets up a basic select2 dropdown on an HTML select element. Dropdown contents should be populated through other means - either using stock Knockout_ bindings or server-side static contents (via cshtml).

    selectOnClose
        Directly maps to select2 option ``selectOnClose``

    openOnFocus
        If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.
    
    allowClear
        Whether or not to allow the current select to be set to null. Directly maps to select2 option ``allowClear``
    
    placeholder
        Placeholder when nothing is selected. Directly maps to select2 option ``placeholder``

datePicker
..........

    updateImmediate
        If true, the datePicker will update the underlying observable on each input. Otherwise, the observable will only be changed when the datePicker loses focus (on :ts:`blur`).

saveImmediately
...............

delaySave
.........
    


Display Bindings
----------------

tooltip
.......

| :ts:`tooltip: tooltipText`
| :ts:`tooltip: {title: note, placement: 'bottom', animation: false}`
|

    Wrapper around the `Bootstrap tooltip component <https://getbootstrap.com/docs/3.3/javascript/#tooltips>`_. Binding can either be simply a string (or observable string), or it can be an object that will be passed directly to the Bootstrap tooltip component.

fadeVisible
...........

| :ts:`fadeVisible: isVisible`
|

    Similar to the Knockout :ts:`visible`, but uses jQuery :ts:`fadeIn/fadeOut` calls to perform the transition.

slideVisible
............

| :ts:`slideVisible: isVisible`
|

    Similar to the Knockout :ts:`visible`, but uses jQuery :ts:`slideIn/slideOut` calls to perform the transition.

moment
......

| :ts:`moment: momentObservable`
| :ts:`moment: momentObservable, format: 'MM/DD/YYYY hh:mm a'`
|

    Controls the text of the element by calling the :ts:`format` method on a moment object. 

momentFromNow
.............

| :ts:`momentFromNow: momentObservable`
| :ts:`momentFromNow: momentObservable, shorten: true`
|

    Controls the text of the element by calling the :ts:`fromNow` method on a moment object. If shorten is true, certain phrases will be slightly shortened. 



Utility Bindings
----------------

let
...

| :ts:`let: {variableName: value}`
|

    The let binding is a somewhat common construct used in Knockout applications, but isn't part of Knockout itself. It effectively allows the creation of variables in the binding context, allowing complex statements which may be used multiple times to be aliased for both clarity of code and better performance.

    .. code-block:: html

        <div class="item">
            <!-- ko let: { showControls: $data.isEditing() || $parent.editingChildren() } -->
            <button data-bind="click: $root.editItem, visible: showControls">Edit</button>
            <span data-bind="text: name"></span>
            <button data-bind="click: $root.deleteItem, visible: showControls">Delete</button>
            <!-- /ko -->
        </div>






Knockout Bindings
=================


Coalesce provides a number of knockout bindings that make common model binding activities much easier. 




Input Bindings
--------------

    select2Ajax
        | :html:`<select data-bind="..."></select>`
        | :ts:`select2Ajax: personId, url: '/api/Person/list', idField: 'personId', textField: 'Name', object: person, allowClear: true`

        Creates a select2 dropdown using the specified url and fields. Additional complimentary bindings include:

            idField
                The name of the field on each item in the results of the AJAX call which contains the ID of the option. The value of this field will be set on the observable specified for the main ``select2Ajax`` binding.

            textField
                The name of the field on each item in the results of the AJAX call which contain the text to be displayed for each option.

            url
                The Coalesce API List or CustomList url to call to populate the contents of the dropdown.

            pageSize
                The number of items to request in each call to the server.

            format
                A string containing the substring "{0}", which will be replaced with the text value of an option in the dropdown list when the option is displayed.
            
            selectionFormat
                A string containing the substring "{0}", which will be replaced with the text value of the selected option of the dropdown list.

            setObject
                If true, the object specified by the :ts:`object` binding will be set to the selected data when an option is chosen in the dropdown

            selectOnClose
                Directly maps to select2 option ``selectOnClose``
                
            allowClear
                Whether or not to allow the current select to be set to null. Directly maps to select2 option ``allowClear``
                
            placeholder
                Placeholder when nothing is selected. Directly maps to select2 option ``placeholder``

            openOnFocus
                If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.

            

    select2AjaxMultiple
        | :html:`<select multiple="multiple" data-bind="..."></select>`
        | :ts:`select2AjaxMultiple: people, url: '/api/Person/list', idField: 'personId', textField: 'Name', itemViewModel: ViewModels.Person`

        Creates a select2 dropdown using the specified url and fields. Additional complimentary bindings include:

            idField
                The name of the field on each item in the results of the AJAX call which contains the ID of the option. The value of this field will be set on the observable specified for the main ``select2Ajax`` binding.

            textField
                The name of the field on each item in the results of the AJAX call which contain the text to be displayed for each option.

            url
                The Coalesce List or CustomList API url to call to populate the contents of the dropdown.

            pageSize
                The number of items to request in each call to the server.

            format
                A string containing the substring "{0}", which will be replaced with the text value of an option in the dropdown list when the option is displayed.
            
            selectionFormat
                A string containing the substring "{0}", which will be replaced with the text value of the selected option of the dropdown list.

            selectOnClose
                Directly maps to select2 option ``selectOnClose``
                
            allowClear
                Whether or not to allow the current select to be set to null. Directly maps to select2 option ``allowClear``
                
            placeholder
                Placeholder when nothing is selected. Directly maps to select2 option ``placeholder``

            openOnFocus
                If true, the dropdown will open when tabbed to. Browser support may be incomplete in some versions of IE.


    select2AjaxText

    select2

    datePicker

    saveImmediately

    delaySave
    


Display Bindings
----------------

    tooltip
        | :ts:`tooltip: tooltipText`
        | :ts:`tooltip: {title: note, placement: 'bottom', animation: false}`

        Wrapper around the `Bootstrap tooltip component <https://getbootstrap.com/docs/3.3/javascript/#tooltips>`_. Binding can either be simply a string (or observable string), or it can be an object that will be passed directly to the Bootstrap tooltip component.

    fadeVisible
        | :ts:`fadeVisible: isVisible`

        Similar to the Knockout :ts:`visible`, but uses jQuery :ts:`fadeIn/fadeOut` calls to perform the transition.

    slideVisible
        | :ts:`slideVisible: isVisible`

        Similar to the Knockout :ts:`visible`, but uses jQuery :ts:`slideIn/slideOut` calls to perform the transition.

    moment
        | :ts:`moment: momentObservable`
        | :ts:`moment: momentObservable, format: 'MM/DD/YYYY hh:mm a'`

        Controls the text of the element by calling the :ts:`format` method on a moment object. 

    momentFromNow
        | :ts:`momentFromNow: momentObservable`
        | :ts:`momentFromNow: momentObservable, shorten: true`

        Controls the text of the element by calling the :ts:`fromNow` method on a moment object. If shorten is true, certain phrases will be slightly shortened. 



Utility Bindings
----------------

    let
        :ts:`let: {variableName: value}`

        The let binding is a somewhat common construct used in Knockout applications, but isn't part of Knockout itself. It effectively allows the creation of variables in the binding context, allowing complex statements which may be used multiple times to be aliased for both clarity of code and better performance.

        .. code-block:: html

            <div class="item">
                <!-- ko let: { showControls: $data.isEditing() || $parent.editingChildren() } -->
                <button data-bind="click: $root.editItem, visible: showControls">Edit</button>
                <span data-bind="text: name"></span>
                <button data-bind="click: $root.deleteItem, visible: showControls">Delete</button>
                <!-- /ko -->
            </div>




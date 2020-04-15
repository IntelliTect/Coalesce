
.. _KoTypeScriptViewModel:


TypeScript ViewModels
---------------------

For each database-mapped type in your model, Coalesce will generate a TypeScript class that provides a multitude of functionality for interacting with the data on the client.

These ViewModels are dependent on Knockout_, and are designed to be used directly from Knockout bindings in your HTML. All data properties on the generated model are Knockout observables.


Base Members
============

    :ts:`includes: string`
        String that will be passed to the server when loading and saving that allows for data trimming via C# Attributes. See :ref:`Includes` for more information.

    :ts:`isChecked: KnockoutObservable<boolean>`
        Flag to use to determine if this item is checked. Only provided for convenience.

    :ts:`isSelected: KnockoutObservable<boolean>`
        Flag to use to determine if this item is selected. Only provided for convenience.


    :ts:`isEditing: KnockoutObservable<boolean>`
        Flag to use to determine if this item is being edited. Only provided for convenience.
    
    :ts:`toggleIsEditing () => void`
        Toggles the :ts:`isEditing` flag.


    :ts:`isExpanded: KnockoutObservable<boolean>`
        Flag to use to determine if this item is expanded. Only provided for convenience.

    :ts:`toggleIsExpanded: () => void`
        Toggles the :ts:`isExpanded` flag.


    :ts:`isVisible: KnockoutObservable<boolean>`
        Flag to use to determine if this item is shown. Only provided for convenience.
    
    :ts:`toggleIsSelected () => void`
        Toggles the :ts:`isSelected` flag.


    :ts:`selectSingle: (): boolean`
        Sets isSelected(true) on this object and clears on the rest of the items in the parent collection.



    :ts:`isDirty: KnockoutObservable<boolean>`
        Dirty Flag. Set when a value on the model changes. Reset when the model is saved or reloaded.

    :ts:`isLoaded: KnockoutObservable<boolean>`
        True once the data has been loaded.

    :ts:`isLoading: KnockoutObservable<boolean>`
        True if the object is loading.


    :ts:`isSaving: KnockoutObservable<boolean>`
        True if the object is currently saving.

    :ts:`isThisOrChildSaving: KnockoutComputed<boolean>`
        Returns true if the current object, or any of its children, are saving.

    :ts:`load: id: any, callback?: (self: T) => void): JQueryPromise<any> | undefined`
        Loads the object from the server based on the id specified. If no id is specified, the current id, is used if one is set.

    :ts:`loadChildren: callback?: () => void) => void`
        Loads any child objects that have an ID set, but not the full object. This is useful when creating an object that has a parent object and the ID is set on the new child.

    :ts:`loadFromDto: data: any, force?: boolean, allowCollectionDeletes?: boolean) => void`
        Loads this object from a data transfer object received from the server. 

        * :ts:`force` - Will override the check against isLoading that is done to prevent recursion.
        * :ts:`allowCollectionDeletes` - Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.


    :ts:`deleteItem: callback?: (self: T) => void): JQueryPromise<any> | undefined`
        Deletes the object without any prompt for confirmation.

    :ts:`deleteItemWithConfirmation: callback?: () => void, message?: string): JQueryPromise<any> | undefined`
        Deletes the object if a prompt for confirmation is answered affirmatively.

    :ts:`errorMessage: KnockoutObservable<string>`
        Contains the error message from the last failed call to the server.


    :ts:`onSave: callback: (self: T) => void): boolean`
        Register a callback to be called when a save is done.
        Returns :ts:`true` if the callback was registered, or :ts:`false` if the callback was already registered.

    :ts:`saveToDto: () => any`
        Saves this object into a data transfer object to send to the server.

    :ts:`save: callback?: (self: T) => void): JQueryPromise<any> | boolean | undefined`
        Saves the object to the server and then calls a callback. Returns false if there are validation errors.


    :ts:`parent: any`
        Parent of this object, if this object was loaded as part of a hierarchy.

    :ts:`parentCollection: KnockoutObservableArray<T>`
        Parent of this object, if this object was loaded as part of list of objects.



    :ts:`editUrl: KnockoutComputed<string>`
        URL to a stock editor for this object.

    :ts:`showEditor: callback?: any): JQueryPromise<any>`
        Displays an editor for the object in a modal dialog.


    :ts:`validate: (): boolean`
        Triggers any validation messages to be shown, and returns a bool that indicates if there are any validation errors.

    :ts:`validationIssues: any`
        ValidationIssues returned from the server when trying to persist data

    :ts:`warnings: KnockoutValidationErrors`
        List of warnings found during validation. Saving is still allowed with warnings present.

    :ts:`errors: KnockoutValidationErrors`
        List of errors found during validation. Any errors present will prevent saving.



Model-Specific Members
======================

    Configuration
        A static configuration object for configuring all instances of the ViewModel's  type is created, as well as an instance configuration object for configuring specific instances of the ViewModel. See (see :ref:`TSModelConfig`) for more information.

        .. code-block:: knockout

            public static coalesceConfig: Coalesce.ViewModelConfiguration<Person>
                = new Coalesce.ViewModelConfiguration<Person>(Coalesce.GlobalConfiguration.viewModel);

            public coalesceConfig: Coalesce.ViewModelConfiguration<Person>
                = new Coalesce.ViewModelConfiguration<Person>(Person.coalesceConfig);

    DataSources
        For each of the :ref:`DataSources` for a model, a class will be added to a namespace named ``ListViewModels.<ClassName>DataSources``. This namespace can always be accessed on both :ts:`ViewModel` and :ts:`ListViewModel` instances via the :ts:`dataSources` property, and class instances can be assigned to the :ts:`dataSource` property.

        .. code-block:: knockout

            public dataSources = ListViewModels.PersonDataSources;
            public dataSource: DataSource<Person> = new this.dataSources.Default();

    Data Properties
        For each exposed property on the underlying EF POCO, a :ts:`KnockoutObservable<T>` property will exist on the TypeScript model. For navigation properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections (including collection navigation properties), these properties will be :ts:`KnockoutObservableArray<T>` objects.

        .. code-block:: knockout

            public personId: KnockoutObservable<number> = ko.observable(null);
            public fullName: KnockoutObservable<string> = ko.observable(null);
            public gender: KnockoutObservable<number> = ko.observable(null);
            public companyId: KnockoutObservable<number> = ko.observable(null);
            public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);
            public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
            public birthDate: KnockoutObservable<moment.Moment> = ko.observable(moment());
    
    .. _TypeScriptViewModelComputedText:
    Computed Text Properties
        For each reference navigation property and each Enum property on your POCO, a :ts:`KnockoutComputed<string>` property will be created that will provide the text to display for that property. For navigation properties, this will be the property on the class annotated with :ref:`ListTextAttribute`.

        .. code-block:: knockout

            public companyText: () => string;
            public genderText: () => string;

    Collection Navigation Property Helpers
        For each collection navigation property on the POCO, the following members will be created:

        - A method that will add a new object to that collection property. If :ts:`autoSave` is specified, the auto-save behavior of the new object will be set to that value. Otherwise, the inherited default will be used (see :ref:`TSModelConfig`)

            .. code-block:: knockout

                public addToAddresses: (autoSave?: boolean) => ViewModels.Address;

        - A :ts:`KnockoutComputed<string>` that evaluates to a relative url for the generated table view that contains only the items that belong to the collection navigation property.
    
            .. code-block:: knockout

                public addressesListUrl: KnockoutComputed<string>;

    Reference Navigation Property Helpers
        For each reference navigation property on the POCO, the following members will be created:

        - A method that will call :ts:`showEditor` on that current value of the navigation property, or on a new instance if the current value is null.
    
            .. code-block:: knockout

                public showCompanyEditor: (callback?: any) => void;

    Instance Method Members
        For each :ref:`Instance Method <ModelMethods>` on your POCO, the members outlined in :ref:`Methods - Generated TypeScript <ModelMethodTypeScript>` will be created.

    Enum Members
        For each :csharp:`enum` property on your POCO, the following will be created:

        - A static array of objects with properties :ts:`id` and :ts:`value` that represent all the values of the enum.
    
            .. code-block:: knockout

                public genderValues: Coalesce.EnumValue[] = [ 
                    { id: 1, value: 'Male' },
                    { id: 2, value: 'Female' },
                    { id: 3, value: 'Other' },
                ];

        - A TypeScript enum that mirrors the C# enum directly. This enum is in a sub-namespace of :ts:`ViewModels` named the same as the class name.
    
            .. code-block:: knockout

                export namespace Person {
                    export enum GenderEnum {
                        Male = 1,
                        Female = 2,
                        Other = 3,
                    };
                }


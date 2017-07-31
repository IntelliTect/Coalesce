
.. _TypeScriptViewModel:


TypeScript ViewModels
---------------------

For each database-mapped type in your model, Coalesce will generate a TypeScript class that provides a multitude of functionality for interacting with the data on the client.

.. _Knockout: http://knockoutjs.com/

These ViewModels are dependent on Knockout_, and are designed to be used directly from Knockout bindings in your HTML. All data properties on the generated model are Knockout observables.


Base Members
============



Model-Specific Members
======================

    Data Properties
        For each exposed property on the underlying EF POCO, a :ts:`KnockoutObservable<T>` property will exist on the TypeScript model. For navigation properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections (including collection navigation properties), these properties will be :ts:`KnockoutObservableArray<T>` objects.

        .. code-block:: typescript

            public personId: KnockoutObservable<number> = ko.observable(null);
            public fullName: KnockoutObservable<string> = ko.observable(null);
            public gender: KnockoutObservable<number> = ko.observable(null);
            public companyId: KnockoutObservable<number> = ko.observable(null);
            public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);
            public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
            public birthDate: KnockoutObservable<moment.Moment> = ko.observable(moment());
    
    .. _TypeScriptViewModelComputedText:
    Computed Text Properties
        For each reference navigation property and each :cs:`enum` property on your POCO, a :ts:`KnockoutComputed<string>` property will be created that will provide the text to display for that property. For navigation properties, this will be the property on the class annotated with :ref:`ListTextAttribute`.

        .. code-block:: typescript

            public companyText: () => string;
            public genderText: () => string;

    Collection Navigation Property Helpers
        For each collection navigation property on the POCO, the following members will be created:

        - A method that will add a new object to that collection property. If :ts:`autoSave` is specified, the auto-save behavior of the new object will be set to that value. Otherwise, the inherited default will be used (see :ref:`TSModelConfig`)

            .. code-block:: typescript

                public addToAddresses: (autoSave?: boolean) => ViewModels.Address;

        - A :ts:`KnockoutComputed<string>` that evaluates to a relative url for the generated table view that contains only the items that belong to the collection navigation property.
    
            .. code-block:: typescript

                public addressesListUrl: KnockoutComputed<string>;

    Reference Navigation Property Helpers
        For each reference navigation property on the POCO, the following members will be created:

        - A method that will load from the server a list of the first 25 possible values of the navigation property, and a :ts:`KnockoutObservableArray<any>` that will store the results. The contents of this array are raw JavaScript objects that contain one property for the primary key of the object, and one property for the evaluated :ref:`ListTextAttribute` of that object.
    
            .. code-block:: typescript

                public loadCompanyValidValues: (callback?: any) => JQueryPromise<any>;
                public companyValidValues: KnockoutObservableArray<any> = ko.observableArray([]);

        - A method that will call :ts:`showEditor` on that current value of the navigation property, or on a new instance if the current value is null.
    
            .. code-block:: typescript

                public showCompanyEditor: (callback?: any) => void;

    Instance Method Members
        For each :ref:`Instance Method <ModelMethods>` on your POCO, the members outlined in :ref:`Methods - Generated TypeScript <ModelMethodTypeScript>` will be created.

    Enum Members
        For each :cs:`Enum` property on your POCO, the following will be created:

        - A static array of objects with properties :ts:`id` and :ts:`value` that represent all the values of the enum.
    
            .. code-block:: typescript

                public genderValues: EnumValue[] = [ 
                    { id: 1, value: 'Male' },
                    { id: 2, value: 'Female' },
                    { id: 3, value: 'Other' },
                ];

        - A TypeScript enum that mirrors the C# enum directly. This enum is in a sub-namespace of :ts:`ViewModels` named the same as the class name.
    
            .. code-block:: typescript

                export namespace Person {
                    export enum GenderEnum {
                        Male = 1,
                        Female = 2,
                        Other = 3,
                    };
                }


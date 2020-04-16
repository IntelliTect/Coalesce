

.. _KoExternalViewModel:

TypeScript External ViewModels
------------------------------

For all :ref:`ExternalTypes` in your model, Coalesce will generate a TypeScript class that provides a barebones representation of that type's properties.

These ViewModels are dependent on Knockout_, and are designed to be used directly from Knockout bindings in your HTML. All data properties on the generated model are Knockout observables.

Base Members
============

    The TypeScript ViewModels for external types do not have a common base class, and do not have any of the behaviors or convenience properties that the regular :ref:`KoTypeScriptViewModels` for database-mapped classes have.


Model-Specific Members
======================

    Data Properties
        For each exposed property on the underlying EF POCO, a :ts:`KnockoutObservable<T>` property will exist on the TypeScript model. For POCO properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections, these properties will be :ts:`KnockoutObservableArray<T>` objects.

        .. code-block:: knockout

            public personId: KnockoutObservable<number> = ko.observable(null);
            public fullName: KnockoutObservable<string> = ko.observable(null);
            public gender: KnockoutObservable<number> = ko.observable(null);
            public companyId: KnockoutObservable<number> = ko.observable(null);
            public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);
            public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
            public birthDate: KnockoutObservable<moment.Moment> = ko.observable(moment());

    Computed Text Properties
        For each Enum property on your POCO, a :ts:`KnockoutComputed<string>` property will be created that will provide the text to display for that property.

        .. code-block:: knockout

            public genderText: () => string;

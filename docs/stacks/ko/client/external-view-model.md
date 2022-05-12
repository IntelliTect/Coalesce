
# TypeScript External ViewModels

For all [External Types](/modeling/model-types/external-types.md) in your model, Coalesce will generate a TypeScript class that provides a bare-bones representation of that type's properties.

These ViewModels are dependent on [Knockout](http://knockoutjs.com/), and are designed to be used directly from Knockout bindings in your HTML. All data properties on the generated model are Knockout observables.

[[toc]]

## Base Members

The TypeScript ViewModels for external types do not have a common base class, and do not have any of the behaviors or convenience properties that the regular [TypeScript ViewModels](/stacks/ko/client/view-model.md) for database-mapped classes have.


## Model-Specific Members


### Data Properties
<Prop def="
public personId: KnockoutObservable<number | null> = ko.observable(null);
public fullName: KnockoutObservable<string | null> = ko.observable(null);
public gender: KnockoutObservable<number | null> = ko.observable(null);
public companyId: KnockoutObservable<number | null> = ko.observable(null);
public company: KnockoutObservable<ViewModels.Company | null> = ko.observable(null);
public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
public birthDate: KnockoutObservable<moment.Moment | null> = ko.observable(moment());" lang="ts" />

For each exposed property on the underlying EF POCO, a `KnockoutObservable<T>` property will exist on the TypeScript model. For navigation properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections (including collection navigation properties), these properties will be `KnockoutObservableArray<T>` objects.


### Enum Members
For each `enum` property on your POCO, the following will be created:

<Prop def="public genderText: KnockoutComputed<string | null>" lang="ts" />

A `KnockoutComputed<string>` property that will provide the text to display for that property.


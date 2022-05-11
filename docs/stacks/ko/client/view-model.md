

# TypeScript ViewModels

For each database-mapped type in your model, Coalesce will generate a TypeScript class that provides a multitude of functionality for interacting with the data on the client.

These ViewModels are dependent on [Knockout](http://knockoutjs.com/), and are designed to be used directly from Knockout bindings in your HTML. All data properties on the generated model are Knockout observables.


## Base Members

<Prop def="includes: string" lang="ts" />

String that will be passed to the server when loading and saving that allows for data trimming via C# Attributes. See [Includes String](/concepts/includes.md) for more information.

<Prop def="isChecked: KnockoutObservable<boolean>" lang="ts" />

Flag to use to determine if this item is checked. Only provided for convenience.

<Prop def="isSelected: KnockoutObservable<boolean>" lang="ts" />

Flag to use to determine if this item is selected. Only provided for convenience.


<Prop def="isEditing: KnockoutObservable<boolean>" lang="ts" />

Flag to use to determine if this item is being edited. Only provided for convenience.

<Prop def="toggleIsEditing () => void" lang="ts" />

Toggles the `isEditing` flag.


<Prop def="isExpanded: KnockoutObservable<boolean>" lang="ts" />

Flag to use to determine if this item is expanded. Only provided for convenience.

<Prop def="toggleIsExpanded: () => void" lang="ts" />

Toggles the `isExpanded` flag.


<Prop def="isVisible: KnockoutObservable<boolean>" lang="ts" />

Flag to use to determine if this item is shown. Only provided for convenience.

<Prop def="toggleIsSelected () => void" lang="ts" />

Toggles the `isSelected` flag.


<Prop def="selectSingle: (): boolean" lang="ts" />

Sets isSelected(true) on this object and clears on the rest of the items in the parent collection.



<Prop def="isDirty: KnockoutObservable<boolean>" lang="ts" />

Dirty Flag. Set when a value on the model changes. Reset when the model is saved or reloaded.

<Prop def="isLoaded: KnockoutObservable<boolean>" lang="ts" />

True once the data has been loaded.

<Prop def="isLoading: KnockoutObservable<boolean>" lang="ts" />

True if the object is loading.


<Prop def="isSaving: KnockoutObservable<boolean>" lang="ts" />

True if the object is currently saving.

<Prop def="isThisOrChildSaving: KnockoutComputed<boolean>" lang="ts" />

Returns true if the current object, or any of its children, are saving.

<Prop def="load: id: any, callback?: (self: T) => void): JQueryPromise<any> | undefined" lang="ts" />

Loads the object from the server based on the id specified. If no id is specified, the current id, is used if one is set.

<Prop def="loadChildren: callback?: () => void) => void" lang="ts" />

Loads any child objects that have an ID set, but not the full object. This is useful when creating an object that has a parent object and the ID is set on the new child.

<Prop def="loadFromDto: data: any, force?: boolean, allowCollectionDeletes?: boolean) => void" lang="ts" />

Loads this object from a data transfer object received from the server. 

    * `force` - Will override the check against isLoading that is done to prevent recursion.
    * `allowCollectionDeletes` - Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.


<Prop def="deleteItem: callback?: (self: T) => void): JQueryPromise<any> | undefined" lang="ts" />

Deletes the object without any prompt for confirmation.

<Prop def="deleteItemWithConfirmation: callback?: () => void, message?: string): JQueryPromise<any> | undefined" lang="ts" />

Deletes the object if a prompt for confirmation is answered affirmatively.

<Prop def="errorMessage: KnockoutObservable<string>" lang="ts" />

Contains the error message from the last failed call to the server.


<Prop def="onSave: callback: (self: T) => void): boolean" lang="ts" />

Register a callback to be called when a save is done.
    Returns `true` if the callback was registered, or `false` if the callback was already registered.

<Prop def="saveToDto: () => any" lang="ts" />

Saves this object into a data transfer object to send to the server.

<Prop def="save: callback?: (self: T) => void): JQueryPromise<any> | boolean | undefined" lang="ts" />

Saves the object to the server and then calls a callback. Returns false if there are validation errors.


<Prop def="parent: any" lang="ts" />

Parent of this object, if this object was loaded as part of a hierarchy.

<Prop def="parentCollection: KnockoutObservableArray<T>" lang="ts" />

Parent of this object, if this object was loaded as part of list of objects.



<Prop def="editUrl: KnockoutComputed<string>" lang="ts" />

URL to a stock editor for this object.

<Prop def="showEditor: callback?: any): JQueryPromise<any>" lang="ts" />

Displays an editor for the object in a modal dialog.


<Prop def="validate: (): boolean" lang="ts" />

Triggers any validation messages to be shown, and returns a bool that indicates if there are any validation errors.

<Prop def="validationIssues: any" lang="ts" />

ValidationIssues returned from the server when trying to persist data

<Prop def="warnings: KnockoutValidationErrors" lang="ts" />

List of warnings found during validation. Saving is still allowed with warnings present.

<Prop def="errors: KnockoutValidationErrors" lang="ts" />

List of errors found during validation. Any errors present will prevent saving.



## Model-Specific Members

Configuration
    A static configuration object for configuring all instances of the ViewModel's  type is created, as well as an instance configuration object for configuring specific instances of the ViewModel. See (see [ViewModel Configuration](/stacks/ko/client/model-config.md)) for more information.

    ``` ts
    public static coalesceConfig: Coalesce.ViewModelConfiguration<Person>
        = new Coalesce.ViewModelConfiguration<Person>(Coalesce.GlobalConfiguration.viewModel);

    public coalesceConfig: Coalesce.ViewModelConfiguration<Person>
        = new Coalesce.ViewModelConfiguration<Person>(Person.coalesceConfig);
    ```

DataSources
    For each of the [Data Sources](/modeling/model-components/data-sources.md) for a model, a class will be added to a namespace named ``ListViewModels.<ClassName>DataSources``. This namespace can always be accessed on both `ViewModel` and `ListViewModel` instances via the `dataSources` property, and class instances can be assigned to the `dataSource` property.

    ``` ts
    public dataSources = ListViewModels.PersonDataSources;
    public dataSource: DataSource<Person> = new this.dataSources.Default();
    ```

Data Properties
    For each exposed property on the underlying EF POCO, a `KnockoutObservable<T>` property will exist on the TypeScript model. For navigation properties, these will be typed with the corresponding TypeScript ViewModel for the other end of the relationship. For collections (including collection navigation properties), these properties will be `KnockoutObservableArray<T>` objects.

    ``` ts
    public personId: KnockoutObservable<number> = ko.observable(null);
    public fullName: KnockoutObservable<string> = ko.observable(null);
    public gender: KnockoutObservable<number> = ko.observable(null);
    public companyId: KnockoutObservable<number> = ko.observable(null);
    public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);
    public addresses: KnockoutObservableArray<ViewModels.Address> = ko.observableArray([]);
    public birthDate: KnockoutObservable<moment.Moment> = ko.observable(moment());
    ```

Computed Text Properties
    For each reference navigation property and each Enum property on your POCO, a `KnockoutComputed<string>` property will be created that will provide the text to display for that property. For navigation properties, this will be the property on the class annotated with [[ListText]](/modeling/model-components/attributes/list-text.md).

    ``` ts
    public companyText: () => string;
    public genderText: () => string;
    ```

Collection Navigation Property Helpers
    For each collection navigation property on the POCO, the following members will be created:

    - A method that will add a new object to that collection property. If `autoSave` is specified, the auto-save behavior of the new object will be set to that value. Otherwise, the inherited default will be used (see [ViewModel Configuration](/stacks/ko/client/model-config.md))

    ``` ts
    public addToAddresses: (autoSave?: boolean) => ViewModels.Address;
    ```

    - A `KnockoutComputed<string>` that evaluates to a relative url for the generated table view that contains only the items that belong to the collection navigation property.

    ``` ts
    public addressesListUrl: KnockoutComputed<string>;
    ```

Reference Navigation Property Helpers
    For each reference navigation property on the POCO, the following members will be created:

    - A method that will call `showEditor` on that current value of the navigation property, or on a new instance if the current value is null.

    ``` ts
    public showCompanyEditor: (callback?: any) => void;
    ```

Instance Method Members
    For each [Instance Method](/modeling/model-components/methods.md) on your POCO, the members outlined in [Methods - Generated TypeScript](/stacks/ko/client/methods.md) will be created.

Enum Members
    For each `enum` property on your POCO, the following will be created:

    - A static array of objects with properties `id` and `value` that represent all the values of the enum.

    ``` ts
    public genderValues: Coalesce.EnumValue[] = [ 
        { id: 1, value: 'Male' },
        { id: 2, value: 'Female' },
        { id: 3, value: 'Other' },
    ];
    ```

    - A TypeScript enum that mirrors the C# enum directly. This enum is in a sub-namespace of `ViewModels` named the same as the class name.

    ``` ts
    export namespace Person {
        export enum GenderEnum {
            Male = 1,
            Female = 2,
            Other = 3,
        };
    }
    ```




# TypeScript ListViewModels

In addition to [TypeScript ViewModels](/stacks/ko/client/view-model.md) for interacting with instances of your data classes in TypeScript, Coalesce will also generated a List ViewModel for loading searched, sorted, paginated data from the server.

These ListViewModels, like the ViewModels, are dependent on Knockout and are designed to be used directly from Knockout bindings in your HTML.

[[toc]]

## Base Members

The following members are defined on `BaseListViewModel<>` and are available to the ListViewModels for all of your model types:

<Prop def="modelKeyName: string" lang="ts" />

Name of the primary key of the model that this list represents.

<Prop def="includes: string" lang="ts" />

String that is used to control loading and serialization on the server. See [Includes String](/concepts/includes.md) for more information.
    

<Prop def="items: KnockoutObservableArray<TItem>" lang="ts" />

The collection of items that have been loaded from the server.

<Prop def="addNewItem: (): TItem" lang="ts" />

Adds a new item to the items collection.
    
<Prop def="deleteItem: (item: TItem): JQueryPromise<any>" lang="ts" />

Deletes an item and removes it from the items collection.


<Prop def="queryString: string" lang="ts" />

An arbitrary URL query string to append to the API call when loading the list of items.
    
<Prop def="search: KnockoutObservable<string>" lang="ts" />

Search criteria for the list. This can be easily bound to with a text box for easy search behavior. See [[Search]](/modeling/model-components/attributes/search.md) for a detailed look at how searching works in Coalesce.

    
<Prop def="isLoading: KnockoutObservable<boolean>" lang="ts" />

True if the list is loading.

<Prop def="isLoaded: KnockoutObservable<boolean>" lang="ts" />

True once the list has been loaded.
    
<Prop def="load: (callback?: any): JQueryPromise<any>" lang="ts" />

Load the list using current parameters for paging, searching, etc Result is placed into the items property.
    
<Prop def="message: KnockoutObservable<string>" lang="ts" />

If a load failed, this is a message about why it failed.
    

<Prop def="getCount: (callback?: any): JQueryPromise<any>" lang="ts" />

Gets the count of items without getting all the items. Result is placed into the count property.

<Prop def="count: KnockoutObservable<number>" lang="ts" />

The result of getCount(), or the total on this page.
    
<Prop def="totalCount: KnockoutObservable<number>" lang="ts" />

Total count of items, even ones that are not on the page.

    
<Prop def="nextPage: (): void" lang="ts" />

Change to the next page.
    
<Prop def="nextPageEnabled: KnockoutComputed<boolean>" lang="ts" />

True if there is another page after the current page.
    
<Prop def="previousPage: (): void" lang="ts" />

Change to the previous page.
    
<Prop def="previousPageEnabled: KnockoutComputed<boolean>" lang="ts" />

True if there is another page before the current page.
    
<Prop def="page: KnockoutObservable<number>" lang="ts" />

Page number. This can be set to get a new page.
    
<Prop def="pageCount: KnockoutObservable<number>" lang="ts" />

Total page count
    
<Prop def="pageSize: KnockoutObservable<number>" lang="ts" />

Number of items on a page.

<Prop def="orderBy: KnockoutObservable<string>" lang="ts" />

Name of a field by which this list will be loaded in ascending order.

If set to `"none"`, default sorting behavior, including behavior defined with use of [[DefaultOrderBy]](/modeling/model-components/attributes/default-order-by.md) in C# POCOs, is suppressed.
    
<Prop def="orderByDescending: KnockoutObservable<string>" lang="ts" />

Name of a field by which this list will be loaded in descending order.
    
<Prop def="orderByToggle: (field: string): void" lang="ts" />

Toggles sorting between ascending, descending, and no order on the specified field.
        

## Model-Specific Members

### Configuration

<Prop def="static coalesceConfig: Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>" lang="ts" id="member-class-config" />

A static configuration object for configuring all instances of the ListViewModel's type is created. See [ViewModel Configuration](/stacks/ko/client/model-config.md).

<Prop def="coalesceConfig: Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>" lang="ts" id="member-instance-config" />

An per-instance configuration object for configuring each specific ListViewModel instance is created. See [ViewModel Configuration](/stacks/ko/client/model-config.md).


### Filter Object
<Prop def="public filter: {
    personId?: string
    firstName?: string
    lastName?: string
    gender?: string
    companyId?: string
} = null;" lang="ts" id="code-filter-object" />

For each exposed scalar property on the underlying EF POCO, `filter` will have a corresponding property. If the `filter` object is set, requests made to the server to retrieve data will be passed all the values in this object via the URL's query string. These parameters will filter the resulting data to only rows where the parameter values match the row's values. For example, if `filter.companyId` is set to a value, only people from that company will be returned.

These parameters all allow for freeform string values, allowing the server to implement any kind of filtering logic desired. The [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source) will perform the following depending on the property type:

@[import-md "after":"MARKER:filter-behaviors", "before":"MARKER:end-filter-behaviors"](../../../modeling/model-components/data-sources.md) 

Example usage:
``` ts
var list = new ListViewModels.PersonList();
list.filter = { lastName: "Erickson" };
list.load();
```


### Static Method Members

<Prop def="public readonly namesStartingWith = new Person.NamesStartingWith(this);
public static NamesStartingWith = class NamesStartingWith extends Coalesce.ClientMethod<PersonList, string[]> { ... };" lang="ts" id="code-static-method-members" />

For each exposed [Static Method](/modeling/model-components/methods.md#static-methods) on your POCO, the members outlined in [Methods - Generated TypeScript](/stacks/ko/client/methods.md) will be created.


### DataSources
<Prop def="
public dataSources = ListViewModels.PersonDataSources;
public dataSource: DataSource<Person> = new this.dataSources.Default();" lang="ts" id="code-data-source-members" />

For each of the [Data Sources](/modeling/model-components/data-sources.md) on the class, a corresponding class will be added to a namespace named `ListViewModels.<ClassName>DataSources`. This namespace can always be accessed on both `ViewModel` and `ListViewModel` instances via the `dataSources` property, and class instances can be assigned to the `dataSource` property.


        
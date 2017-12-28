
.. _TypeScriptListViewModel:


TypeScript ListViewModels
-------------------------

In addition to :ref:`TypeScriptViewModel` for interacting with instances of your data classes in TypeScript, Coalesce will also generated a List ViewModel for loading searched, sorted, pagninated data from the server.

.. _Knockout: http://knockoutjs.com/

These ListViewModels, like the ViewModels, are dependent on Knockout_ and are designed to be used directly from Knockout bindings in your HTML.


Base Members
============

    The following members are defined on :ts:`BaseListViewModel<>` and are available to the ListViewModels for all of your model types:

    :ts:`modelKeyName: string`
        Name of the primary key of the model that this list represents.

    :ts:`includes: string`
        String that is used to control loading and serialization on the server. See :ref:`Includes` for more information.
        

    :ts:`items: KnockoutObservableArray<TItem>`
        The collection of items that have been loaded from the server.

    :ts:`addNewItem: (): TItem`
        Adds a new item to the items collection.
        
    :ts:`deleteItem: (item: TItem): JQueryPromise<any>`
        Deletes an item and removes it from the items collection.


    :ts:`queryString: string`
        Query string to append to the API call when loading the list of items. If :ts:`query` is non-null, this value will not be used. See :ref:`below <ListViewModelQuery>` for more information about :ts:`query`.
        
    :ts:`search: KnockoutObservable<string>`
        Search criteria for the list. This can be easily bound to with a text box for easy search behavior. See :ref:`Searching` for a detailed look at how searching works in Coalesce.

        
    :ts:`isLoading: KnockoutObservable<boolean>`
        True if the list is loading.

    :ts:`isLoaded: KnockoutObservable<boolean>`
        True once the list has been loaded.
        
    :ts:`load: (callback?: any): JQueryPromise<any>`
        Load the list using current parameters for paging, searching, etc Result is placed into the items property.
        
    :ts:`message: KnockoutObservable<string>`
        If a load failed, this is a message about why it failed.
        

    :ts:`getCount: (callback?: any): JQueryPromise<any>`
        Gets the count of items without getting all the items. Result is placed into the count property.

    :ts:`count: KnockoutObservable<number>`
        The result of getCount(), or the total on this page.
        
    :ts:`totalCount: KnockoutObservable<number>`
        Total count of items, even ones that are not on the page.

        
    :ts:`nextPage: (): void`
        Change to the next page.
        
    :ts:`nextPageEnabled: KnockoutComputed<boolean>`
        True if there is another page after the current page.
        
    :ts:`previousPage: (): void`
        Change to the previous page.
        
    :ts:`previousPageEnabled: KnockoutComputed<boolean>`
        True if there is another page before the current page.
        
    :ts:`page: KnockoutObservable<number>`
        Page number. This can be set to get a new page.
        
    :ts:`pageCount: KnockoutObservable<number>`
        Total page count
        
    :ts:`pageSize: KnockoutObservable<number>`
        Number of items on a page.
        
.. _TypeScriptListViewModelOrderBy:
    :ts:`orderBy: KnockoutObservable<string>`
        Name of a field by which this list will be loaded in ascending order.

        If set to :ts:`"none"`, default sorting behavior, including behavior defined with use of :csharp:`[DefaultOrderBy]` in C# POCOs, is suppressed.
        
    :ts:`orderByDescending: KnockoutObservable<string>`
        Name of a field by which this list will be loaded in descending order.
        
    :ts:`orderByToggle: (field: string): void`
        Toggles sorting between ascending, descending, and no order on the specified field.
        

    :ts:`csvUploadUi: (callback?: () => void): void`
        Prompts to the user for a file to upload as a CSV.
        
    :ts:`downloadAllCsvUrl: KnockoutComputed<string>`
        Returns URL to download a CSV for the current list with all items.

        

Model-Specific Members
======================

    Configuration
        A static configuration object for configuring all instances of the ListViewModel's  type is created, as well as an instance configuration object for configuring specific instances of the ListViewModel. See (see :ref:`TSModelConfig`) for more information.

        .. code-block:: typescript

            public static coalesceConfig = new Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>(Coalesce.GlobalConfiguration.listViewModel);

            public coalesceConfig = new Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>(PersonList.coalesceConfig);

    .. _ListViewModelQuery:

    Query Object
        For each exposed value type instance property on the underlying EF POCO, a property named :ts:`query` will have a strongly-typed property declaration generated for that property. If the :ts:`query` object is set, requests made to the server to retrieve data will be passed all the values in this object via the URL's query string. These parameters will filter the resulting data to only rows where the parameter values match the row's values. For example, if :ts:`query.companyId` is set to a value, only people from that company will be returned. There is also always a property on this object named :ts:`where` that accepts a freeform `LINQ Dynamic <https://github.com/kahanu/System.Linq.Dynamic/wiki>`_ query.
        
        .. code-block:: typescript

            public query: {
                where?: string;
                personId?: number
                firstName?: string
                lastName?: string
                gender?: number
                companyId?: number
            } = null;


        .. code-block:: typescript

            var list = new ListViewModels.PersonList();
            list.query = {
                where: 'Company.Name == "Acme"'
                lastName: "Erickson",
            };
            list.load();

    Static Method Members
        For each :ref:`Static Method <ModelMethods>` on your POCO, the members outlined in :ref:`Methods - Generated TypeScript <ModelMethodTypeScript>` will be created.

    DataSources
        For each of the :ref:`CustomDataSources` on the class, an enum value will be added to an enum named ``ListViewModels.<ClassName>DataSources``. This enum can always be accessed on both :ts:`ViewModel` and :ts:`ListViewModel` instances via the :ts:`dataSources` property, and enum values can be assigned to the :ts:`dataSource` property.

        .. code-block:: typescript

            module ListViewModels {
                export enum PersonDataSources {
                    Default,
                    IncludeFamily,
                    NamesStartingWithA,
                }

                export class PersonList extends Coalesce.BaseListViewModel<PersonList, ViewModels.Person> {
                    public dataSources = PersonDataSources;
                    public dataSource: PersonDataSources = PersonDataSources.Default;
                }
            }
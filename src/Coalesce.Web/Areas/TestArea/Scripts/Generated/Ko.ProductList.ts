/// <reference path="../../../../typings/tsd.d.ts" />
/// <reference path="../../../../scripts/Intellitect/intellitect.utilities.ts" />
/// <reference path="../../../../scripts/Intellitect/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.Product.ts" />

// Knockout List View Model for: Product
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2016

var baseUrl = baseUrl || '';

module TestArea.ListViewModels {
    export var areaUrl = areaUrl || ((false) ? baseUrl : baseUrl + 'TestArea/');
    // Add an enum for all methods that are static and IQueryable
    export enum ProductDataSources {
            Default,
        }
    export class ProductList {
        // Query string to limit the list of items.
        public queryString: string = "";
        // Object that is passed as the query parameters.
        public query: any = null;
        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: ProductDataSources = ProductDataSources.Default;
        // String the represents the child object to load 
        public includes: string = "";
        // List of items. This the main collection.
        public items: KnockoutObservableArray<TestArea.ViewModels.Product> = ko.observableArray([]);
        // Load the list.
		public load: (callback?: any) => void;
        // Deletes an item.
		public deleteItem: (item: TestArea.ViewModels.Product) => void;
        // True if the collection is loading.
		public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Gets the count of items without getting all the items. Data put into count.
		public getCount: (callback?: any) => void;
        // The result of getCount() or the total on this page.
		public count: KnockoutObservable<number> = ko.observable(null);
        // Total count of items, even ones that are not on the page.
   		public totalCount: KnockoutObservable<number> = ko.observable(null);
        // Total page count
   		public pageCount: KnockoutObservable<number> = ko.observable(null);
        // Page number. This can be set to get a new page.
   		public page: KnockoutObservable<number> = ko.observable(1);
        // Number of items on a page.
   		public pageSize: KnockoutObservable<number> = ko.observable(10);
        // If a load failed, this is a message about why it failed.
   		public message: KnockoutObservable<string> = ko.observable(null);
        // Search criteria for the list. This can be exposed as a text box for searching.
   		public search: KnockoutObservable<string> = ko.observable("");
        // If there is another page, this is true.
        public nextPageEnabled: KnockoutComputed<boolean>
        // If there is a previous page, this is true.
        public previousPageEnabled: KnockoutComputed<boolean>;
        // Gets the next page.
        public nextPage: () => void;
        // Gets the previous page.
        public previousPage: () => void;

        // True once the data has been loaded.
		public isLoaded: KnockoutObservable<boolean> = ko.observable(false);

        // Valid values
            constructor() {
            var self = this; 
            var searchTimeout: number = 0;

            // Load the collection
            self.load = function(callback?: any) {
                intellitect.utilities.showBusy();
                if(self.query) {
                    self.queryString = $.param(self.query);
                }
                self.isLoading(true);
                $.ajax({ method: "GET",
                         url: areaUrl + "api/Product/List?includes=" + self.includes + "&page=" + self.page()
                                + "&pageSize=" + self.pageSize() + "&search=" + self.search()
                                + "&listDataSource=" + ProductDataSources[self.listDataSource] + "&" + self.queryString,
                        xhrFields: { withCredentials: true } })
                .done(function(data) {
                    if (data.WasSuccessful){
                        self.items.removeAll();
                        for (var i in data.List) {
                            var model = new TestArea.ViewModels.Product(data.List[i]);
                            model.includes = self.includes;
                            model.onDelete(itemDeleted);
                            self.items.push(model);
                        }
                        self.count(data.List.length);
                        self.totalCount(data.TotalCount);
                        self.pageCount(data.PageCount);
                        self.page(data.Page);
                        self.message(data.Message)
                        self.isLoaded(true);
                        if ($.isFunction(callback)) callback(self);
                    }else{
                        self.message(data.Message);
                        self.isLoaded(false);
                    }
                })
                .fail(function() {
                    alert("Could not get list of Product items.");
                })
                .always(function() {
                    intellitect.utilities.hideBusy();
                    self.isLoading(false);
                });
            };

            // Paging
            self.nextPage = function() {
                if (self.nextPageEnabled()) {
                    self.page(self.page() + 1);
                    self.load();
                }
            }
            self.nextPageEnabled = ko.computed(function() {
                if (self.page() < self.pageCount()) return true;
                return false;
            })
            self.previousPage = function() {
                if (self.previousPageEnabled()) {
                    self.page(self.page() - 1);
                    self.load();
                }
            }
            self.previousPageEnabled = ko.computed(function() {
                if (self.page() > 1) return true;
                return false;
            })


            // Load the count
            self.getCount = function(callback?: any) {
                intellitect.utilities.showBusy();
                if (self.query) {
                    self.queryString = $.param(self.query);
                }
                $.ajax({ method: "GET",
                         url: areaUrl + "api/Product/count?" + "listDataSource=" + ProductDataSources[self.listDataSource] + "&" + self.queryString,
                         xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.count(data);
                    if ($.isFunction(callback)) callback();
                })
                .fail(function() {
                    alert("Could not get count of Product items.");
                })
                .always(function() {
                    intellitect.utilities.hideBusy();
                });
            };

            // Callback for when an item is deleted
            function itemDeleted(item) {
                self.items.remove(item);
            }

            // Deletes an item and removes it from the array.
            self.deleteItem = function(item: TestArea.ViewModels.Product)
            {
                item.deleteItem();
            };

            self.pageSize.subscribe(function () {
                if (self.isLoaded()){
                    self.load();
                }
            });
            self.search.subscribe(function () {
                if (searchTimeout) {
                    clearTimeout(searchTimeout);
                }
                searchTimeout = setTimeout(function() {
                    searchTimeout = 0;
                    self.load();
                }, 300);
            });

    // Method Implementations
        }
    }
}
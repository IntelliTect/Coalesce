/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.Case.ts" />

// Knockout List View Model for: Case
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2016

var baseUrl = baseUrl || '';

module ListViewModels {
    export var areaUrl = areaUrl || ((true) ? baseUrl : baseUrl + '/');
    // Add an enum for all methods that are static and IQueryable
    export enum CaseDataSources {
            Default,
            GetAllOpenCases,
        }
    export class CaseList {
        // Query string to limit the list of items.
        public queryString: string = "";
        // Object that is passed as the query parameters.
        public query: any = null;
        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: CaseDataSources = CaseDataSources.Default;
        // String the represents the child object to load 
        public includes: string = "";
        // List of items. This the main collection.
        public items: KnockoutObservableArray<ViewModels.Case> = ko.observableArray([]);
        // Load the list.
		public load: (callback?: any) => void;
        // Deletes an item.
		public deleteItem: (item: ViewModels.Case) => void;
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
        // Specify the DTO that should be returned - must be a fully qualified type name
        public dto: KnockoutObservable<string> = ko.observable("");
        // If there is another page, this is true.
        public nextPageEnabled: KnockoutComputed<boolean>
        // If there is a previous page, this is true.
        public previousPageEnabled: KnockoutComputed<boolean>;
        // Gets the next page.
        public nextPage: () => void;
        // Gets the previous page.
        public previousPage: () => void;
        // Control order of results
        public orderBy: KnockoutObservable<string> = ko.observable("");
        public orderByDescending: KnockoutObservable<string> = ko.observable("");

        // True once the data has been loaded.
		public isLoaded: KnockoutObservable<boolean> = ko.observable(false);

        // Valid values
        public assignedToValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadAssignedToValidValues: (callback: any) => void;
        public reportedByValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadReportedByValidValues: (callback: any) => void;
        public caseProductsValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCaseProductsValidValues: (callback: any) => void;
        public devTeamAssignedValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadDevTeamAssignedValidValues: (callback: any) => void;
            // Call server method (GetAllOpenCasesCount)
        public getAllOpenCasesCount: (callback?: any, reload?: boolean) => void;
        // Result of server method (GetAllOpenCasesCount)
        public getAllOpenCasesCountResult: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetAllOpenCasesCount) is being called
        public getAllOpenCasesCountIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetAllOpenCasesCount) if it fails.
        public getAllOpenCasesCountMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetAllOpenCasesCount) was successful.
        public getAllOpenCasesCountWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetAllOpenCasesCount)
        public getAllOpenCasesCountUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (GetAllOpenCasesCount)
        public getAllOpenCasesCountModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
        // Call server method (RandomizeDatesAndStatus)
        public randomizeDatesAndStatus: (callback?: any, reload?: boolean) => void;
        // Result of server method (RandomizeDatesAndStatus)
        public randomizeDatesAndStatusResult: KnockoutObservable<any> = ko.observable();
        // True while the server method (RandomizeDatesAndStatus) is being called
        public randomizeDatesAndStatusIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (RandomizeDatesAndStatus) if it fails.
        public randomizeDatesAndStatusMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (RandomizeDatesAndStatus) was successful.
        public randomizeDatesAndStatusWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (RandomizeDatesAndStatus)
        public randomizeDatesAndStatusUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (RandomizeDatesAndStatus)
        public randomizeDatesAndStatusModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
        // Call server method (GetAllOpenCases)
        public getAllOpenCases: (callback?: any, reload?: boolean) => void;
        // Result of server method (GetAllOpenCases)
        public getAllOpenCasesResult: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetAllOpenCases) is being called
        public getAllOpenCasesIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetAllOpenCases) if it fails.
        public getAllOpenCasesMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetAllOpenCases) was successful.
        public getAllOpenCasesWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetAllOpenCases)
        public getAllOpenCasesUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (GetAllOpenCases)
        public getAllOpenCasesModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
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

                var url = areaUrl + "api/Case/List?includes=" + self.includes + "&page=" + self.page()
                            + "&pageSize=" + self.pageSize() + "&search=" + self.search()
                            + "&orderBy=" + self.orderBy() + "&orderByDescending=" + self.orderByDescending()
                            + "&listDataSource=";
    
                if (typeof self.listDataSource === "string") url += self.listDataSource;
                else url += CaseDataSources[self.listDataSource];

                if (self.queryString !== null && self.queryString !== "") url += "&" + self.queryString;

                $.ajax({ method: "GET",
                         url: url,
                        xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.items.removeAll();
                    for (var i in data.list) {
                        var model = new ViewModels.Case(data.list[i]);
                        model.includes = self.includes;
                        model.onDelete(itemDeleted);
                        self.items.push(model);
                    }
                    self.count(data.list.length);
                    self.totalCount(data.totalCount);
                    self.pageCount(data.pageCount);
                    self.page(data.page);
                    self.message(data.message)
                    self.isLoaded(true);
                    if ($.isFunction(callback)) callback(self);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.message(errorMsg);
                    self.isLoaded(false);
                    
                    alert("Could not get list of Case items: " + errorMsg);
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
                         url: areaUrl + "api/Case/count?" + "listDataSource=" + CaseDataSources[self.listDataSource] + "&" + self.queryString,
                         xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.count(data);
                    if ($.isFunction(callback)) callback();
                })
                .fail(function() {
                    alert("Could not get count of Case items.");
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
            self.deleteItem = function(item: ViewModels.Case)
            {
                item.deleteItem();
            };

            self.pageSize.subscribe(function () {
                if (self.isLoaded()){
                    self.load();
                }
            });
            self.page.subscribe(function () {
                if (self.isLoaded() && !self.isLoading()){
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

            self.getAllOpenCasesCount = function(callback?: any, reload: boolean = true){
                self.getAllOpenCasesCountIsLoading(true);
                self.getAllOpenCasesCountMessage('');
                self.getAllOpenCasesCountWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: areaUrl + "api/Case/GetAllOpenCasesCount",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getAllOpenCasesCountResult(data.object);
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.getAllOpenCasesCountWasSuccessful(false);
                    self.getAllOpenCasesCountMessage(errorMsg);

					//alert("Could not call method getAllOpenCasesCount: " + errorMsg);
				})
				.always(function() {
                    self.getAllOpenCasesCountIsLoading(false);
				});
            }

            self.getAllOpenCasesCountUi = function(callback?: any) {
                                self.getAllOpenCasesCount(callback);
            }

            self.getAllOpenCasesCountModal = function(callback?: any) {
                    self.getAllOpenCasesCountUi(callback);
            }
            

            
            self.randomizeDatesAndStatus = function(callback?: any, reload: boolean = true){
                self.randomizeDatesAndStatusIsLoading(true);
                self.randomizeDatesAndStatusMessage('');
                self.randomizeDatesAndStatusWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: areaUrl + "api/Case/RandomizeDatesAndStatus",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.randomizeDatesAndStatusResult(data.object);
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.randomizeDatesAndStatusWasSuccessful(false);
                    self.randomizeDatesAndStatusMessage(errorMsg);

					//alert("Could not call method randomizeDatesAndStatus: " + errorMsg);
				})
				.always(function() {
                    self.randomizeDatesAndStatusIsLoading(false);
				});
            }

            self.randomizeDatesAndStatusUi = function(callback?: any) {
                                self.randomizeDatesAndStatus(callback);
            }

            self.randomizeDatesAndStatusModal = function(callback?: any) {
                    self.randomizeDatesAndStatusUi(callback);
            }
            

            
            self.getAllOpenCases = function(callback?: any, reload: boolean = true){
                self.getAllOpenCasesIsLoading(true);
                self.getAllOpenCasesMessage('');
                self.getAllOpenCasesWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: areaUrl + "api/Case/GetAllOpenCases",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getAllOpenCasesResult(data.object);
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.getAllOpenCasesWasSuccessful(false);
                    self.getAllOpenCasesMessage(errorMsg);

					//alert("Could not call method getAllOpenCases: " + errorMsg);
				})
				.always(function() {
                    self.getAllOpenCasesIsLoading(false);
				});
            }

            self.getAllOpenCasesUi = function(callback?: any) {
                                self.getAllOpenCases(callback);
            }

            self.getAllOpenCasesModal = function(callback?: any) {
                    self.getAllOpenCasesUi(callback);
            }
            

                    }
    }

    export namespace CaseList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
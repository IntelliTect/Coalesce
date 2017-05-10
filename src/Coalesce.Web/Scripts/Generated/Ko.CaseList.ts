/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.Case.ts" />

// Knockout List View Model for: Case
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum CaseDataSources {
            Default,
            GetAllOpenCases,
        }
    export class CaseList extends BaseListViewModel<CaseList, ViewModels.Case> {
        protected modelName = "Case";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/Case";
        public modelKeyName = "caseKey";
        public dataSources = CaseDataSources;
        public itemClass = ViewModels.Case;

        public query: {
            where?: string;
            caseKey?:number;
            title?:String;
            description?:String;
            openedAt?:moment.Moment;
            assignedToId?:number;
            reportedById?:number;
            severity?:String;
            status?:number;
            devTeamAssignedId?:number;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: CaseDataSources = CaseDataSources.Default;

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
        // Result of server method (GetAllOpenCasesCount) strongly typed in a observable.
        public getAllOpenCasesCountResult: KnockoutObservable<number> = ko.observable(null);
        // Result of server method (GetAllOpenCasesCount) simply wrapped in an observable.
        public getAllOpenCasesCountResultRaw: KnockoutObservable<any> = ko.observable();
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
        // Result of server method (RandomizeDatesAndStatus) strongly typed in a observable.
        public randomizeDatesAndStatusResult: KnockoutObservable<any> = ko.observable(null);
        // Result of server method (RandomizeDatesAndStatus) simply wrapped in an observable.
        public randomizeDatesAndStatusResultRaw: KnockoutObservable<any> = ko.observable();
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
        // Result of server method (GetAllOpenCases) strongly typed in a observable.
        public getAllOpenCasesResult: KnockoutObservableArray<ViewModels.Case> = ko.observableArray([]);
        // Result of server method (GetAllOpenCases) simply wrapped in an observable.
        public getAllOpenCasesResultRaw: KnockoutObservable<any> = ko.observable();
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
        
        // Call server method (GetCaseSummary)
        // Returns a list of summary information about Cases
        public getCaseSummary: (callback?: any, reload?: boolean) => void;
        // Result of server method (GetCaseSummary) strongly typed in a observable.
        public getCaseSummaryResult: KnockoutObservable<ViewModels.CaseSummary> = ko.observable(null);
        // Result of server method (GetCaseSummary) simply wrapped in an observable.
        public getCaseSummaryResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetCaseSummary) is being called
        public getCaseSummaryIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetCaseSummary) if it fails.
        public getCaseSummaryMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetCaseSummary) was successful.
        public getCaseSummaryWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetCaseSummary)
        public getCaseSummaryUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (GetCaseSummary)
        public getCaseSummaryModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        

        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Case(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations

            self.getAllOpenCasesCount = function(callback?: any, reload: boolean = true){
                self.getAllOpenCasesCountIsLoading(true);
                self.getAllOpenCasesCountMessage('');
                self.getAllOpenCasesCountWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Case/GetAllOpenCasesCount",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getAllOpenCasesCountResultRaw(data.object);
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
                         url: self.areaUrl + "api/Case/RandomizeDatesAndStatus",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.randomizeDatesAndStatusResultRaw(data.object);
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
                         url: self.areaUrl + "api/Case/GetAllOpenCases",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getAllOpenCasesResultRaw(data.object);
                    if (self.getAllOpenCasesResult()){
                            RebuildArray(self.getAllOpenCasesResult, data.object, 'caseKey', ViewModels.Case, self, true);
                    }
                    
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
            

            
            self.getCaseSummary = function(callback?: any, reload: boolean = true){
                self.getCaseSummaryIsLoading(true);
                self.getCaseSummaryMessage('');
                self.getCaseSummaryWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Case/GetCaseSummary",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getCaseSummaryResultRaw(data.object);
                    if (!self.getCaseSummaryResult()){
                        self.getCaseSummaryResult(new ViewModels.CaseSummary());
                    }
                    self.getCaseSummaryResult().loadFromDto(data.object);
                    
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.getCaseSummaryWasSuccessful(false);
                    self.getCaseSummaryMessage(errorMsg);

					//alert("Could not call method getCaseSummary: " + errorMsg);
				})
				.always(function() {
                    self.getCaseSummaryIsLoading(false);
				});
            }

            self.getCaseSummaryUi = function(callback?: any) {
                                self.getCaseSummary(callback);
            }

            self.getCaseSummaryModal = function(callback?: any) {
                    self.getCaseSummaryUi(callback);
            }
            

                    }
    }

    export namespace CaseList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
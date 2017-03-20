/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.Person.ts" />

// Knockout List View Model for: Person
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum PersonDataSources {
            Default,
            BorCPeople,
            NamesStartingWithAWithCases,
        }
    export class PersonList extends BaseListViewModel<PersonList, ViewModels.Person> {
        protected modelName = "Person";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/Person";
        public dataSources = PersonDataSources;

        public query: {
            where?: string;
            personId?:number;
            title?:number;
            firstName?:String;
            lastName?:String;
            email?:String;
            gender?:number;
            personStatsId?:number;
            name?:String;
            companyId?:number;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: PersonDataSources = PersonDataSources.Default;

        // Valid values
        public personStatsValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadPersonStatsValidValues: (callback: any) => void;
        public companyValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCompanyValidValues: (callback: any) => void;
            // Call server method (Add)
        public add: (numberOne: number, numberTwo: number, callback?: any, reload?: boolean) => void;
        // Result of server method (Add) strongly typed in a observable.
        public addResult: KnockoutObservable<number> = ko.observable(null);
        // Result of server method (Add) simply wrapped in an observable.
        public addResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (Add) is being called
        public addIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (Add) if it fails.
        public addMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (Add) was successful.
        public addWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (Add)
        public addUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (Add)
        public addModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        public addWithArgs: (args?: PersonList.AddArgs, callback?: any) => void;
        
        public addArgs = new PersonList.AddArgs(); 
        
        // Call server method (GetUser)
        public getUser: (callback?: any, reload?: boolean) => void;
        // Result of server method (GetUser) strongly typed in a observable.
        public getUserResult: KnockoutObservable<string> = ko.observable(null);
        // Result of server method (GetUser) simply wrapped in an observable.
        public getUserResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetUser) is being called
        public getUserIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetUser) if it fails.
        public getUserMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetUser) was successful.
        public getUserWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetUser)
        public getUserUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (GetUser)
        public getUserModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
        // Call server method (GetUserPublic)
        public getUserPublic: (callback?: any, reload?: boolean) => void;
        // Result of server method (GetUserPublic) strongly typed in a observable.
        public getUserPublicResult: KnockoutObservable<string> = ko.observable(null);
        // Result of server method (GetUserPublic) simply wrapped in an observable.
        public getUserPublicResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetUserPublic) is being called
        public getUserPublicIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetUserPublic) if it fails.
        public getUserPublicMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetUserPublic) was successful.
        public getUserPublicWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetUserPublic)
        public getUserPublicUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (GetUserPublic)
        public getUserPublicModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
        // Call server method (NamesStartingWith)
        public namesStartingWith: (characters: String, callback?: any, reload?: boolean) => void;
        // Result of server method (NamesStartingWith) strongly typed in a observable.
        public namesStartingWithResult: KnockoutObservableArray<string> = ko.observableArray([]);
        // Result of server method (NamesStartingWith) simply wrapped in an observable.
        public namesStartingWithResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (NamesStartingWith) is being called
        public namesStartingWithIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (NamesStartingWith) if it fails.
        public namesStartingWithMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (NamesStartingWith) was successful.
        public namesStartingWithWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (NamesStartingWith)
        public namesStartingWithUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (NamesStartingWith)
        public namesStartingWithModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        public namesStartingWithWithArgs: (args?: PersonList.NamesStartingWithArgs, callback?: any) => void;
        
        public namesStartingWithArgs = new PersonList.NamesStartingWithArgs(); 
        
        // Call server method (NamesStartingWithPublic)
        public namesStartingWithPublic: (characters: String, callback?: any, reload?: boolean) => void;
        // Result of server method (NamesStartingWithPublic) strongly typed in a observable.
        public namesStartingWithPublicResult: KnockoutObservableArray<string> = ko.observableArray([]);
        // Result of server method (NamesStartingWithPublic) simply wrapped in an observable.
        public namesStartingWithPublicResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (NamesStartingWithPublic) is being called
        public namesStartingWithPublicIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (NamesStartingWithPublic) if it fails.
        public namesStartingWithPublicMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (NamesStartingWithPublic) was successful.
        public namesStartingWithPublicWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (NamesStartingWithPublic)
        public namesStartingWithPublicUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (NamesStartingWithPublic)
        public namesStartingWithPublicModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        public namesStartingWithPublicWithArgs: (args?: PersonList.NamesStartingWithPublicArgs, callback?: any) => void;
        
        public namesStartingWithPublicArgs = new PersonList.NamesStartingWithPublicArgs(); 
        
        // Call server method (NamesStartingWithAWithCases)
        public namesStartingWithAWithCases: (callback?: any, reload?: boolean) => void;
        // Result of server method (NamesStartingWithAWithCases) strongly typed in a observable.
        public namesStartingWithAWithCasesResult: KnockoutObservableArray<ViewModels.Person> = ko.observableArray([]);
        // Result of server method (NamesStartingWithAWithCases) simply wrapped in an observable.
        public namesStartingWithAWithCasesResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (NamesStartingWithAWithCases) is being called
        public namesStartingWithAWithCasesIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (NamesStartingWithAWithCases) if it fails.
        public namesStartingWithAWithCasesMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (NamesStartingWithAWithCases) was successful.
        public namesStartingWithAWithCasesWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (NamesStartingWithAWithCases)
        public namesStartingWithAWithCasesUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (NamesStartingWithAWithCases)
        public namesStartingWithAWithCasesModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
        // Call server method (BorCPeople)
        public borCPeople: (callback?: any, reload?: boolean) => void;
        // Result of server method (BorCPeople) strongly typed in a observable.
        public borCPeopleResult: KnockoutObservableArray<ViewModels.Person> = ko.observableArray([]);
        // Result of server method (BorCPeople) simply wrapped in an observable.
        public borCPeopleResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (BorCPeople) is being called
        public borCPeopleIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (BorCPeople) if it fails.
        public borCPeopleMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (BorCPeople) was successful.
        public borCPeopleWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (BorCPeople)
        public borCPeopleUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (BorCPeople)
        public borCPeopleModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        

        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Person(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations

            self.add = function(numberOne: number, numberTwo: number, callback?: any, reload: boolean = true){
                self.addIsLoading(true);
                self.addMessage('');
                self.addWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/Add",
                         data: {
                        numberOne: numberOne, 
                        numberTwo: numberTwo
                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.addResultRaw(data.object);
                    self.addResult(data.object);
                    
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.addWasSuccessful(false);
                    self.addMessage(errorMsg);

					//alert("Could not call method add: " + errorMsg);
				})
				.always(function() {
                    self.addIsLoading(false);
				});
            }

            self.addUi = function(callback?: any) {
                var numberOne: number = parseFloat(prompt('Number One'));
                var numberTwo: number = parseFloat(prompt('Number Two'));
                                self.add(numberOne, numberTwo, callback);
            }

            self.addModal = function(callback?: any) {
                $('#method-Add').modal();
                $('#method-Add').on('shown.bs.modal', function() {
                    $('#method-Add .btn-ok').unbind('click');
                    $('#method-Add .btn-ok').click(function()
                    {
                        self.addWithArgs(null, callback);
                        $('#method-Add').modal('hide');
                    });
                });
            }
            
            self.addWithArgs = function(args?: PersonList.AddArgs, callback?: any) {
                if (!args) args = self.addArgs;
                self.add(args.numberOne(), args.numberTwo(), callback);
            }

            
            self.getUser = function(callback?: any, reload: boolean = true){
                self.getUserIsLoading(true);
                self.getUserMessage('');
                self.getUserWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/GetUser",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getUserResultRaw(data.object);
                    self.getUserResult(data.object);
                    
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.getUserWasSuccessful(false);
                    self.getUserMessage(errorMsg);

					//alert("Could not call method getUser: " + errorMsg);
				})
				.always(function() {
                    self.getUserIsLoading(false);
				});
            }

            self.getUserUi = function(callback?: any) {
                                self.getUser(callback);
            }

            self.getUserModal = function(callback?: any) {
                    self.getUserUi(callback);
            }
            

            
            self.getUserPublic = function(callback?: any, reload: boolean = true){
                self.getUserPublicIsLoading(true);
                self.getUserPublicMessage('');
                self.getUserPublicWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/GetUserPublic",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.getUserPublicResultRaw(data.object);
                    self.getUserPublicResult(data.object);
                    
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.getUserPublicWasSuccessful(false);
                    self.getUserPublicMessage(errorMsg);

					//alert("Could not call method getUserPublic: " + errorMsg);
				})
				.always(function() {
                    self.getUserPublicIsLoading(false);
				});
            }

            self.getUserPublicUi = function(callback?: any) {
                                self.getUserPublic(callback);
            }

            self.getUserPublicModal = function(callback?: any) {
                    self.getUserPublicUi(callback);
            }
            

            
            self.namesStartingWith = function(characters: String, callback?: any, reload: boolean = true){
                self.namesStartingWithIsLoading(true);
                self.namesStartingWithMessage('');
                self.namesStartingWithWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/NamesStartingWith",
                         data: {
                        characters: characters
                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.namesStartingWithResultRaw(data.object);
                    self.namesStartingWithResult(data.object);
                    
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.namesStartingWithWasSuccessful(false);
                    self.namesStartingWithMessage(errorMsg);

					//alert("Could not call method namesStartingWith: " + errorMsg);
				})
				.always(function() {
                    self.namesStartingWithIsLoading(false);
				});
            }

            self.namesStartingWithUi = function(callback?: any) {
                var characters: String = prompt('Characters');
                                self.namesStartingWith(characters, callback);
            }

            self.namesStartingWithModal = function(callback?: any) {
                $('#method-NamesStartingWith').modal();
                $('#method-NamesStartingWith').on('shown.bs.modal', function() {
                    $('#method-NamesStartingWith .btn-ok').unbind('click');
                    $('#method-NamesStartingWith .btn-ok').click(function()
                    {
                        self.namesStartingWithWithArgs(null, callback);
                        $('#method-NamesStartingWith').modal('hide');
                    });
                });
            }
            
            self.namesStartingWithWithArgs = function(args?: PersonList.NamesStartingWithArgs, callback?: any) {
                if (!args) args = self.namesStartingWithArgs;
                self.namesStartingWith(args.characters(), callback);
            }

            
            self.namesStartingWithPublic = function(characters: String, callback?: any, reload: boolean = true){
                self.namesStartingWithPublicIsLoading(true);
                self.namesStartingWithPublicMessage('');
                self.namesStartingWithPublicWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/NamesStartingWithPublic",
                         data: {
                        characters: characters
                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.namesStartingWithPublicResultRaw(data.object);
                    self.namesStartingWithPublicResult(data.object);
                    
                    if (reload) {
                      self.load(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
				})
				.fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.namesStartingWithPublicWasSuccessful(false);
                    self.namesStartingWithPublicMessage(errorMsg);

					//alert("Could not call method namesStartingWithPublic: " + errorMsg);
				})
				.always(function() {
                    self.namesStartingWithPublicIsLoading(false);
				});
            }

            self.namesStartingWithPublicUi = function(callback?: any) {
                var characters: String = prompt('Characters');
                                self.namesStartingWithPublic(characters, callback);
            }

            self.namesStartingWithPublicModal = function(callback?: any) {
                $('#method-NamesStartingWithPublic').modal();
                $('#method-NamesStartingWithPublic').on('shown.bs.modal', function() {
                    $('#method-NamesStartingWithPublic .btn-ok').unbind('click');
                    $('#method-NamesStartingWithPublic .btn-ok').click(function()
                    {
                        self.namesStartingWithPublicWithArgs(null, callback);
                        $('#method-NamesStartingWithPublic').modal('hide');
                    });
                });
            }
            
            self.namesStartingWithPublicWithArgs = function(args?: PersonList.NamesStartingWithPublicArgs, callback?: any) {
                if (!args) args = self.namesStartingWithPublicArgs;
                self.namesStartingWithPublic(args.characters(), callback);
            }

            
            self.namesStartingWithAWithCases = function(callback?: any, reload: boolean = true){
                self.namesStartingWithAWithCasesIsLoading(true);
                self.namesStartingWithAWithCasesMessage('');
                self.namesStartingWithAWithCasesWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/NamesStartingWithAWithCases",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.namesStartingWithAWithCasesResultRaw(data.object);
                    if (self.namesStartingWithAWithCasesResult()){
                            RebuildArray(self.namesStartingWithAWithCasesResult, data.object, 'personId', ViewModels.Person, self, true);
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
                    self.namesStartingWithAWithCasesWasSuccessful(false);
                    self.namesStartingWithAWithCasesMessage(errorMsg);

					//alert("Could not call method namesStartingWithAWithCases: " + errorMsg);
				})
				.always(function() {
                    self.namesStartingWithAWithCasesIsLoading(false);
				});
            }

            self.namesStartingWithAWithCasesUi = function(callback?: any) {
                                self.namesStartingWithAWithCases(callback);
            }

            self.namesStartingWithAWithCasesModal = function(callback?: any) {
                    self.namesStartingWithAWithCasesUi(callback);
            }
            

            
            self.borCPeople = function(callback?: any, reload: boolean = true){
                self.borCPeopleIsLoading(true);
                self.borCPeopleMessage('');
                self.borCPeopleWasSuccessful(null);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/BorCPeople",
                         data: {

                    },
                         xhrFields: { withCredentials: true } })
				.done(function(data) {
					self.borCPeopleResultRaw(data.object);
                    if (self.borCPeopleResult()){
                            RebuildArray(self.borCPeopleResult, data.object, 'personId', ViewModels.Person, self, true);
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
                    self.borCPeopleWasSuccessful(false);
                    self.borCPeopleMessage(errorMsg);

					//alert("Could not call method borCPeople: " + errorMsg);
				})
				.always(function() {
                    self.borCPeopleIsLoading(false);
				});
            }

            self.borCPeopleUi = function(callback?: any) {
                                self.borCPeople(callback);
            }

            self.borCPeopleModal = function(callback?: any) {
                    self.borCPeopleUi(callback);
            }
            

                    }
    }

    export namespace PersonList {
        // Classes for use in method calls to support data binding for input for arguments
        export class AddArgs {
            public numberOne: KnockoutObservable<number> = ko.observable(null);
            public numberTwo: KnockoutObservable<number> = ko.observable(null);
        }
        export class NamesStartingWithArgs {
            public characters: KnockoutObservable<string> = ko.observable(null);
        }
        export class NamesStartingWithPublicArgs {
            public characters: KnockoutObservable<string> = ko.observable(null);
        }
    }
}

/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ListViewModels {
    
    export namespace PersonDataSources {
        
        /** People whose last name starts with B or c */
        export class BOrCPeople extends Coalesce.DataSource<ViewModels.Person> {
        }
        export class NamesStartingWithAWithCases extends Coalesce.DataSource<ViewModels.Person> {
        }
        export class WithoutCases extends Coalesce.DataSource<ViewModels.Person> {
        }
        export const Default = WithoutCases;
    }
    
    export class PersonList extends Coalesce.BaseListViewModel<ViewModels.Person> {
        public readonly modelName: string = "Person";
        public readonly apiController: string = "/Person";
        public modelKeyName: string = "personId";
        public itemClass: new () => ViewModels.Person = ViewModels.Person;
        
        public filter: {
            personId?: string;
            title?: string;
            firstName?: string;
            lastName?: string;
            email?: string;
            gender?: string;
            birthDate?: string;
            lastBath?: string;
            nextUpgrade?: string;
            companyId?: string;
        } | null = null;
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof PersonDataSources = PersonDataSources;
        
        /** The data source on the server to use when retrieving objects. Valid values are in this.dataSources. */
        public dataSource: Coalesce.DataSource<ViewModels.Person> = new this.dataSources.Default();
        
        /** Configuration for all instances of PersonList. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>(Coalesce.GlobalConfiguration.listViewModel);
        
        /** Configuration for this PersonList instance. */
        public coalesceConfig: Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>
            = new Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>(PersonList.coalesceConfig);
        
        
        /** 
            Methods and properties for invoking server method Add.
            
            Adds two numbers.
            
            This comment also includes multiple lines so I can test multi-line xmldoc comments.
        */
        public readonly add = new PersonList.Add(this);
        public static Add = class Add extends Coalesce.ClientMethod<PersonList, number> {
            public readonly name = 'Add';
            public readonly verb = 'POST';
            
            /** Calls server method (Add) with the given arguments */
            public invoke = (numberOne: number | null, numberTwo: number | null, callback?: (result: number) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ numberOne: numberOne, numberTwo: numberTwo }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new Add.Args(); 
            public static Args = class Args {
                public numberOne: KnockoutObservable<number | null> = ko.observable(null);
                public numberTwo: KnockoutObservable<number | null> = ko.observable(null);
            };
            
            /** Calls server method (Add) with an instance of Add.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: number) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.numberOne(), args.numberTwo(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: number) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                $promptVal = prompt('Number One');
                if ($promptVal === null) return;
                var numberOne: number = parseInt($promptVal);
                $promptVal = prompt('Number Two');
                if ($promptVal === null) return;
                var numberTwo: number = parseInt($promptVal);
                return this.invoke(numberOne, numberTwo, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: number) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** 
            Methods and properties for invoking server method GetUser.
            
            Returns the user name
        */
        public readonly getUser = new PersonList.GetUser(this);
        public static GetUser = class GetUser extends Coalesce.ClientMethod<PersonList, string> {
            public readonly name = 'GetUser';
            public readonly verb = 'POST';
            
            /** Calls server method (GetUser) with the given arguments */
            public invoke = (callback?: (result: string) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({  }, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: string) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** Methods and properties for invoking server method PersonCount. */
        public readonly personCount = new PersonList.PersonCount(this);
        public static PersonCount = class PersonCount extends Coalesce.ClientMethod<PersonList, number> {
            public readonly name = 'PersonCount';
            public readonly verb = 'GET';
            
            /** Calls server method (PersonCount) with the given arguments */
            public invoke = (lastNameStartsWith: string | null, callback?: (result: number) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ lastNameStartsWith: lastNameStartsWith }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new PersonCount.Args(); 
            public static Args = class Args {
                public lastNameStartsWith: KnockoutObservable<string | null> = ko.observable(null);
            };
            
            /** Calls server method (PersonCount) with an instance of PersonCount.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: number) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.lastNameStartsWith(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: number) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                $promptVal = prompt('Last Name Starts With');
                if ($promptVal === null) return;
                var lastNameStartsWith: string = $promptVal;
                return this.invoke(lastNameStartsWith, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: number) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
            
            /** URL for method 'PersonCount' */
            public url: KnockoutComputed<string> = ko.pureComputed(() => 
                this.parent.coalesceConfig.baseApiUrl() + this.parent.apiController + '/' + this.name + '?'
                + $.param({ lastNameStartsWith: this.args.lastNameStartsWith })
            );
        };
        
        /** Methods and properties for invoking server method RemovePersonById. */
        public readonly removePersonById = new PersonList.RemovePersonById(this);
        public static RemovePersonById = class RemovePersonById extends Coalesce.ClientMethod<PersonList, boolean> {
            public readonly name = 'RemovePersonById';
            public readonly verb = 'DELETE';
            
            /** Calls server method (RemovePersonById) with the given arguments */
            public invoke = (id: number | null, callback?: (result: boolean) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ id: id }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new RemovePersonById.Args(); 
            public static Args = class Args {
                public id: KnockoutObservable<number | null> = ko.observable(null);
            };
            
            /** Calls server method (RemovePersonById) with an instance of RemovePersonById.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: boolean) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.id(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: boolean) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                $promptVal = prompt('Id');
                if ($promptVal === null) return;
                var id: number = parseInt($promptVal);
                return this.invoke(id, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: boolean) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** 
            Methods and properties for invoking server method GetUserPublic.
            
            Returns the user name
        */
        public readonly getUserPublic = new PersonList.GetUserPublic(this);
        public static GetUserPublic = class GetUserPublic extends Coalesce.ClientMethod<PersonList, string> {
            public readonly name = 'GetUserPublic';
            public readonly verb = 'POST';
            
            /** Calls server method (GetUserPublic) with the given arguments */
            public invoke = (callback?: (result: string) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({  }, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: string) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** 
            Methods and properties for invoking server method NamesStartingWith.
            
            Gets all the first names starting with the characters.
        */
        public readonly namesStartingWith = new PersonList.NamesStartingWith(this);
        public static NamesStartingWith = class NamesStartingWith extends Coalesce.ClientMethod<PersonList, string[]> {
            public readonly name = 'NamesStartingWith';
            public readonly verb = 'POST';
            public result: KnockoutObservableArray<string> = ko.observableArray([]);
            
            /** Calls server method (NamesStartingWith) with the given arguments */
            public invoke = (characters: string | null, callback?: (result: string[]) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ characters: characters }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new NamesStartingWith.Args(); 
            public static Args = class Args {
                public characters: KnockoutObservable<string | null> = ko.observable(null);
            };
            
            /** Calls server method (NamesStartingWith) with an instance of NamesStartingWith.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: string[]) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.characters(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: string[]) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                $promptVal = prompt('Characters');
                if ($promptVal === null) return;
                var characters: string = $promptVal;
                return this.invoke(characters, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: string[]) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** Methods and properties for invoking server method MethodWithStringArrayParameter. */
        public readonly methodWithStringArrayParameter = new PersonList.MethodWithStringArrayParameter(this);
        public static MethodWithStringArrayParameter = class MethodWithStringArrayParameter extends Coalesce.ClientMethod<PersonList, string[]> {
            public readonly name = 'MethodWithStringArrayParameter';
            public readonly verb = 'POST';
            public result: KnockoutObservableArray<string> = ko.observableArray([]);
            
            /** Calls server method (MethodWithStringArrayParameter) with the given arguments */
            public invoke = (strings: string[] | null, callback?: (result: string[]) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ strings: strings }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new MethodWithStringArrayParameter.Args(); 
            public static Args = class Args {
                public strings: KnockoutObservableArray<string> = ko.observableArray([]);
            };
            
            /** Calls server method (MethodWithStringArrayParameter) with an instance of MethodWithStringArrayParameter.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: string[]) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.strings(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: string[]) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                var strings: null = null;
                return this.invoke(strings, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: string[]) => void, reload: boolean = true) => {
                this.result(data.object);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** 
            Methods and properties for invoking server method MethodWithEntityParameter.
            
            Gets all the first names starting with the characters.
        */
        public readonly methodWithEntityParameter = new PersonList.MethodWithEntityParameter(this);
        public static MethodWithEntityParameter = class MethodWithEntityParameter extends Coalesce.ClientMethod<PersonList, ViewModels.Person> {
            public readonly name = 'MethodWithEntityParameter';
            public readonly verb = 'POST';
            
            /** Calls server method (MethodWithEntityParameter) with the given arguments */
            public invoke = (person: ViewModels.Person | null, callback?: (result: ViewModels.Person) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ person: person?.saveToDto() }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new MethodWithEntityParameter.Args(); 
            public static Args = class Args {
                public person: KnockoutObservable<ViewModels.Person | null> = ko.observable(null);
            };
            
            /** Calls server method (MethodWithEntityParameter) with an instance of MethodWithEntityParameter.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: ViewModels.Person) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.person(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: ViewModels.Person) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                var person: null = null;
                return this.invoke(person, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ItemResult, jqXHR: JQuery.jqXHR, callback?: (result: ViewModels.Person) => void, reload: boolean = true) => {
                if (!this.result()) {
                    this.result(new ViewModels.Person(data.object));
                } else {
                    this.result().loadFromDto(data.object);
                }
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        /** 
            Methods and properties for invoking server method SearchPeople.
            
            Gets people matching the criteria, paginated by parameter 'page'.
        */
        public readonly searchPeople = new PersonList.SearchPeople(this);
        public static SearchPeople = class SearchPeople extends Coalesce.ClientListMethod<PersonList, ViewModels.Person[]> {
            public readonly name = 'SearchPeople';
            public readonly verb = 'POST';
            public result: KnockoutObservableArray<ViewModels.Person> = ko.observableArray([]);
            
            /** Calls server method (SearchPeople) with the given arguments */
            public invoke = (criteria: ViewModels.PersonCriteria | null, page: number | null, callback?: (result: ViewModels.Person[]) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invokeWithData({ criteria: criteria?.saveToDto(), page: page }, callback, reload);
            };
            
            /** Object that can be easily bound to fields to allow data entry for the method's parameters */
            public args = new SearchPeople.Args(); 
            public static Args = class Args {
                public criteria: KnockoutObservable<ViewModels.PersonCriteria | null> = ko.observable(null);
                public page: KnockoutObservable<number | null> = ko.observable(null);
            };
            
            /** Calls server method (SearchPeople) with an instance of SearchPeople.Args, or the value of this.args if not specified. */
            public invokeWithArgs = (args = this.args, callback?: (result: ViewModels.Person[]) => void, reload: boolean = true): JQueryPromise<any> => {
                return this.invoke(args.criteria(), args.page(), callback, reload);
            }
            
            /** Invokes the method after displaying a browser-native prompt for each argument. */
            public invokeWithPrompts = (callback?: (result: ViewModels.Person[]) => void, reload: boolean = true): JQueryPromise<any> | undefined => {
                var $promptVal: string | null = null;
                $promptVal = prompt('Page');
                if ($promptVal === null) return;
                var page: number = parseInt($promptVal);
                var criteria: null = null;
                return this.invoke(criteria, page, callback, reload);
            };
            
            protected loadResponse = (data: Coalesce.ListResult, jqXHR: JQuery.jqXHR, callback?: (result: ViewModels.Person[]) => void, reload: boolean = true) => {
                Coalesce.KnockoutUtilities.RebuildArray(this.result, data.list || [], 'personId', ViewModels.Person, this, true);
                if (reload) {
                    var result = this.result();
                    this.parent.load(typeof(callback) == 'function' ? () => callback(result) : undefined);
                } else if (typeof(callback) == 'function') {
                    callback(this.result());
                }
            };
        };
        
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Person(newItem, parent);
        
        constructor() {
            super();
        }
    }
}

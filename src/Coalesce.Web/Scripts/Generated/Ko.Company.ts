/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: Company
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {

	export class Company extends BaseViewModel<Company>
    {
        protected modelName = "Company";
        protected modelDisplayName = "Company";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected primaryKeyName = "companyId";
        protected apiUrlBase = "api/Company";
        protected viewUrlBase = "Company";
        public dataSources = ListViewModels.CompanyDataSources;


        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public dataSource: ListViewModels.CompanyDataSources = ListViewModels.CompanyDataSources.Default;
        
    
        // Observables
        public companyId: KnockoutObservable<number> = ko.observable(null);
        public name: KnockoutObservable<string> = ko.observable(null);
        public address1: KnockoutObservable<string> = ko.observable(null);
        public address2: KnockoutObservable<string> = ko.observable(null);
        public city: KnockoutObservable<string> = ko.observable(null);
        public state: KnockoutObservable<string> = ko.observable(null);
        public zipCode: KnockoutObservable<string> = ko.observable(null);
        public employees: KnockoutObservableArray<ViewModels.Person> = ko.observableArray([]);
        public altName: KnockoutObservable<string> = ko.observable(null);

       
        // Create computeds for display for objects
        
        public addToEmployees: () => Person;
        // List Object model for Employees. Allows for loading subsets of data.
        public employeesList: (loadImmediate?: boolean) => ListViewModels.PersonList;

        public EmployeesListUrl: () => void; 
                // Pops up a stock editor for this object.
        public showEditor: (callback?: any) => void;



        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: Company) {};

        constructor(newItem?: any, parent?: any){
            super();
            var self = this;
            self.parent = parent;
            self.myId;
            // Call an init function that allows for proper inheritance.
            if ($.isFunction(self.init)){
                self.init(self);
            }
            
            ko.validation.init({
                grouping: {
                    deep: true,
                    live: true,
                    observable: true
                }
            });

            // SetupValidation {
            
            self.errors = ko.validation.group([
                self.companyId,
                self.name,
                self.address1,
                self.address2,
                self.city,
                self.state,
                self.zipCode,
                self.employees,
                self.altName,
            ]);
            self.warnings = ko.validation.group([
            ]);

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return self.areaUrl + self.viewUrlBase + "/CreateEdit?id=" + self.companyId();
            });

            // Create computeds for display for objects


            // Load the ViewModel object from the DTO. 
            // Force: Will override the check against isLoading that is done to prevent recursion. False is default.
            // AllowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
			self.loadFromDto = function(data: any, force: boolean = false, allowCollectionDeletes: boolean = true) {
				if (!data || (!force && self.isLoading())) return;
				self.isLoading(true);
				// Set the ID 
				self.myId = data.companyId;
				// Load the lists of other objects
                if (data.employees != null) {
				    // Merge the incoming array
				    RebuildArray(self.employees, data.employees, 'personId', Person, self, allowCollectionDeletes);
				} 
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.

				// The rest of the objects are loaded now.
				self.companyId(data.companyId);
				self.name(data.name);
				self.address1(data.address1);
				self.address2(data.address2);
				self.city(data.city);
				self.state(data.state);
				self.zipCode(data.zipCode);
				self.altName(data.altName);
                if (self.afterLoadFromDto){
                    self.afterLoadFromDto();
                }
				self.isLoading(false);
				self.isDirty(false);
                self.validate();
			};

    	    // Save the object into a DTO
			self.saveToDto = function() {
				var dto: any = {};
				dto.companyId = self.companyId();

    	        dto.name = self.name();
    	        dto.address1 = self.address1();
    	        dto.address2 = self.address2();
    	        dto.city = self.city();
    	        dto.state = self.state();
    	        dto.zipCode = self.zipCode();

				return dto;
			}

            // Methods to add to child collections

            self.addToEmployees = function() {
                var newItem = new Person();
                newItem.parent = self;
                newItem.parentCollection = self.employees;
                newItem.isExpanded(true);
                newItem.companyId(self.companyId());
                self.employees.push(newItem);
                return newItem;
            }
            
            // List Object model for Employees. Allows for loading subsets of data.
            var _employeesList: ListViewModels.PersonList = null;
            self.employeesList = function(loadImmediate = true) {
                if (!_employeesList){
                    _employeesList = new ListViewModels.PersonList();
                    if (loadImmediate) loadEmployeesList();
                    self.companyId.subscribe(loadEmployeesList)
                }
                return _employeesList;
            }
            function loadEmployeesList() {
                if (self.companyId()){
                    _employeesList.queryString = "CompanyId=" + self.companyId();
                    _employeesList.load();
                }
            }

            // Save on changes
            function setupSubscriptions() {
                self.name.subscribe(self.autoSave);
                self.address1.subscribe(self.autoSave);
                self.address2.subscribe(self.autoSave);
                self.city.subscribe(self.autoSave);
                self.state.subscribe(self.autoSave);
                self.zipCode.subscribe(self.autoSave);
            }  

            // Create variables for ListEditorApiUrls
            self.EmployeesListUrl = ko.computed({
                read: function() {
                         return self.areaUrl + 'Person/table?companyId=' + self.companyId();
                },
                deferEvaluation: true
            });
            // Create loading function for Valid Values


            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                if (loadingCount == 0 && $.isFunction(callback)){
                    callback();
                }
            };



            // Load all the valid values in parallel.
            self.loadValidValues = function(callback) {
                if ($.isFunction(callback)) callback();
            };

            // Enumeration Lookups.


            // Method Implementations


            // This stuff needs to be done after everything else is set up.
            // Complex Type Observables

            // Make sure everything is defined before we call this.
            setupSubscriptions();

            if (newItem) {
                if ($.isNumeric(newItem)) self.load(newItem);
                else self.loadFromDto(newItem, true);
            }



        }
    }





    export namespace Company {

        // Classes for use in method calls to support data binding for input for arguments
    }
}
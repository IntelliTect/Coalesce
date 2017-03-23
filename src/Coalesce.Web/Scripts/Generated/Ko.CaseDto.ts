/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: CaseDto
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {

	export class CaseDto extends BaseViewModel<CaseDto>
    {
        protected modelName = "CaseDto";
        protected modelDisplayName = "Case Dto";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected primaryKeyName = "caseId";
        protected apiUrlBase = "api/CaseDto";
        protected viewUrlBase = "CaseDto";
        public dataSources = ListViewModels.CaseDtoDataSources;


        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public dataSource: ListViewModels.CaseDtoDataSources = ListViewModels.CaseDtoDataSources.Default;
        
    
        // Observables
        public caseId: KnockoutObservable<number> = ko.observable(null);
        public title: KnockoutObservable<string> = ko.observable(null);
        public assignedToName: KnockoutObservable<string> = ko.observable(null);

       
        // Create computeds for display for objects
        

                // Pops up a stock editor for this object.
        public showEditor: (callback?: any) => void;



        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: CaseDto) {};

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
                self.caseId,
                self.title,
                self.assignedToName,
            ]);
            self.warnings = ko.validation.group([
            ]);

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return self.areaUrl + self.viewUrlBase + "/CreateEdit?id=" + self.caseId();
            });

            // Create computeds for display for objects


            // Load the ViewModel object from the DTO. 
            // Force: Will override the check against isLoading that is done to prevent recursion. False is default.
            // AllowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
			self.loadFromDto = function(data: any, force: boolean = false, allowCollectionDeletes: boolean = true) {
				if (!data || (!force && self.isLoading())) return;
				self.isLoading(true);
				// Set the ID 
				self.myId = data.caseId;
				// Load the lists of other objects
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.

				// The rest of the objects are loaded now.
				self.caseId(data.caseId);
				self.title(data.title);
				self.assignedToName(data.assignedToName);
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
				dto.caseId = self.caseId();

    	        dto.title = self.title();

				return dto;
			}

            // Methods to add to child collections


            // Save on changes
            function setupSubscriptions() {
                self.title.subscribe(self.autoSave);
            }  

            // Create variables for ListEditorApiUrls
            // Create loading function for Valid Values


            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                var obj;
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





    export namespace CaseDto {

        // Classes for use in method calls to support data binding for input for arguments
    }
}
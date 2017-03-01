/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: Case
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {

	export class Case extends BaseViewModel<Case>
    {
        protected modelName = "Case";
        protected modelDisplayName = "Case";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected primaryKeyName = "caseKey";
        protected apiUrlBase = "api/Case";
        protected viewUrlBase = "Case";
        public dataSources = ListViewModels.CaseDataSources;


        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public dataSource: ListViewModels.CaseDataSources = ListViewModels.CaseDataSources.Default;
        
    
        // Observables
        // The Primary key for the Case object
        public caseKey: KnockoutObservable<number> = ko.observable(null);
        public title: KnockoutObservable<string> = ko.observable(null);
        public description: KnockoutObservable<string> = ko.observable(null);
        public openedAt: KnockoutObservable<any> = ko.observable(moment());
        public assignedToId: KnockoutObservable<number> = ko.observable(null);
        public assignedTo: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        public reportedById: KnockoutObservable<number> = ko.observable(null);
        public reportedBy: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        public attachment: KnockoutObservable<string> = ko.observable(null);
        public severity: KnockoutObservable<string> = ko.observable(null);
        public status: KnockoutObservable<number> = ko.observable(null);
        // Text value for enumeration Status
        public statusText: KnockoutComputed<string> = ko.computed<string>(() => "");
        public caseProducts: KnockoutObservableArray<ViewModels.CaseProduct> = ko.observableArray([]);
        public products: KnockoutObservableArray<ViewModels.Product> = ko.observableArray([]);  // Many to Many Collection
        public devTeamAssignedId: KnockoutObservable<number> = ko.observable(null);
        public devTeamAssigned: KnockoutObservable<ViewModels.DevTeam> = ko.observable(null);

       
        // Create computeds for display for objects
        public assignedToText: () => string;
        public reportedByText: () => string;
        public devTeamAssignedText: () => string;
        

        public CaseProductsListUrl: () => void; 
                public assignedToValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadAssignedToValidValues: (callback?: any) => void;
        public reportedByValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadReportedByValidValues: (callback?: any) => void;
        public caseProductsValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCaseProductsValidValues: (callback?: any) => void;
        public devTeamAssignedValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadDevTeamAssignedValidValues: (callback?: any) => void;
        // Pops up a stock editor for this object.
        public showEditor: (callback?: any) => void;
        public showAssignedToEditor: (callback?: any) => void;
        public showReportedByEditor: (callback?: any) => void;
        public showDevTeamAssignedEditor: (callback?: any) => void;


        public statusValues: EnumValue[] = [ 
            { id: 0, value: 'Open' },
            { id: 1, value: 'In Progress' },
            { id: 2, value: 'Resolved' },
            { id: 3, value: 'Closed No Solution' },
            { id: 4, value: 'Cancelled' },
        ];

        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: Case) {};

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
            self.title = self.title.extend({ required: {params: true, message: "You must enter a title for the case."} });
			self.openedAt = self.openedAt.extend({ moment: { unix: true } });
            
            self.errors = ko.validation.group([
                self.caseKey,
                self.title,
                self.description,
                self.openedAt,
                self.assignedToId,
                self.assignedTo,
                self.reportedById,
                self.reportedBy,
                self.attachment,
                self.severity,
                self.status,
                self.caseProducts,
                self.devTeamAssignedId,
                self.devTeamAssigned,
            ]);
            self.warnings = ko.validation.group([
            ]);

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return self.areaUrl + self.viewUrlBase + "/CreateEdit?id=" + self.caseKey();
            });

            // Create computeds for display for objects
			self.assignedToText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.assignedTo() && self.assignedTo().name()) {
					return self.assignedTo().name().toString();
				} else {
					return "None";
				}
			});
			self.reportedByText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.reportedBy() && self.reportedBy().name()) {
					return self.reportedBy().name().toString();
				} else {
					return "None";
				}
			});
			self.devTeamAssignedText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.devTeamAssigned() && self.devTeamAssigned().name()) {
					return self.devTeamAssigned().name().toString();
				} else {
					return "None";
				}
			});


            // Load the ViewModel object from the DTO. 
            // Force: Will override the check against isLoading that is done to prevent recursion. False is default.
            // AllowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
			self.loadFromDto = function(data: any, force: boolean = false, allowCollectionDeletes: boolean = true) {
				if (!data || (!force && self.isLoading())) return;
				self.isLoading(true);
				// Set the ID 
				self.myId = data.caseKey;
				// Load the lists of other objects
                if (data.caseProducts != null) {
				    // Merge the incoming array
				    RebuildArray(self.caseProducts, data.caseProducts, 'caseProductId', CaseProduct, self, allowCollectionDeletes);
                    // Add many-to-many collection
                    var objs = [];
                    $.each(data.caseProducts, function(index, item) {
                        if (item.product){
                            objs.push(item.product);
                        }
                    });
				    RebuildArray(self.products, objs, 'productId', Product, self, allowCollectionDeletes);
				} 
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.
				if (!data.assignedTo) { 
					if (data.assignedToId != self.assignedToId()) {
                        self.assignedTo(null);
                    }
                }else {
                    if (!self.assignedTo()){
					    self.assignedTo(new Person(data.assignedTo, self));
				    }else{
					    self.assignedTo().loadFromDto(data.assignedTo);
				    }
                    if (self.parent && self.parent.myId == self.assignedTo().myId && intellitect.utilities.getClassName(self.parent) == intellitect.utilities.getClassName(self.assignedTo()))
                    {
                        self.parent.loadFromDto(data.assignedTo, undefined, false);
                    }
                }
				if (!data.reportedBy) { 
					if (data.reportedById != self.reportedById()) {
                        self.reportedBy(null);
                    }
                }else {
                    if (!self.reportedBy()){
					    self.reportedBy(new Person(data.reportedBy, self));
				    }else{
					    self.reportedBy().loadFromDto(data.reportedBy);
				    }
                    if (self.parent && self.parent.myId == self.reportedBy().myId && intellitect.utilities.getClassName(self.parent) == intellitect.utilities.getClassName(self.reportedBy()))
                    {
                        self.parent.loadFromDto(data.reportedBy, undefined, false);
                    }
                }
				if (!data.devTeamAssigned) { 
					if (data.devTeamAssignedId != self.devTeamAssignedId()) {
                        self.devTeamAssigned(null);
                    }
                }else {
                    if (!self.devTeamAssigned()){
					    self.devTeamAssigned(new DevTeam(data.devTeamAssigned, self));
				    }else{
					    self.devTeamAssigned().loadFromDto(data.devTeamAssigned);
				    }
                    if (self.parent && self.parent.myId == self.devTeamAssigned().myId && intellitect.utilities.getClassName(self.parent) == intellitect.utilities.getClassName(self.devTeamAssigned()))
                    {
                        self.parent.loadFromDto(data.devTeamAssigned, undefined, false);
                    }
                }

				// The rest of the objects are loaded now.
				self.caseKey(data.caseKey);
				self.title(data.title);
				self.description(data.description);
                if (data.openedAt == null) self.openedAt(null);
				else if (self.openedAt() == null || self.openedAt() == false || !self.openedAt().isSame(moment(data.openedAt))){
				    self.openedAt(moment(data.openedAt));
				}
				self.assignedToId(data.assignedToId);
				self.reportedById(data.reportedById);
				self.attachment(data.attachment);
				self.severity(data.severity);
				self.status(data.status);
				self.devTeamAssignedId(data.devTeamAssignedId);
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
				dto.caseKey = self.caseKey();

    	        dto.title = self.title();
    	        dto.description = self.description();
				if (!self.openedAt()) dto.OpenedAt = null;
				else dto.openedAt = self.openedAt().format('YYYY-MM-DDTHH:mm:ssZZ');
				dto.assignedToId = self.assignedToId();
				if (!dto.assignedToId && self.assignedTo()) {
				    dto.assignedToId = self.assignedTo().personId();
				}
				dto.reportedById = self.reportedById();
				if (!dto.reportedById && self.reportedBy()) {
				    dto.reportedById = self.reportedBy().personId();
				}
    	        dto.attachment = self.attachment();
    	        dto.severity = self.severity();
    	        dto.status = self.status();
				dto.devTeamAssignedId = self.devTeamAssignedId();
				if (!dto.devTeamAssignedId && self.devTeamAssigned()) {
				    dto.devTeamAssignedId = self.devTeamAssigned().devTeamId();
				}

				return dto;
			}

            // Methods to add to child collections


            // Save on changes
            function setupSubscriptions() {
                self.title.subscribe(self.autoSave);
                self.description.subscribe(self.autoSave);
                self.openedAt.subscribe(self.autoSave);
                self.assignedToId.subscribe(self.autoSave);
                self.assignedTo.subscribe(self.autoSave);
                self.reportedById.subscribe(self.autoSave);
                self.reportedBy.subscribe(self.autoSave);
                self.attachment.subscribe(self.autoSave);
                self.severity.subscribe(self.autoSave);
                self.status.subscribe(self.autoSave);
                self.devTeamAssignedId.subscribe(self.autoSave);
                self.devTeamAssigned.subscribe(self.autoSave);
                            self.products.subscribe(function(changes){
                    if (!self.isLoading() && changes.length > 0){
                        for (var i in changes){
                            var change:any = changes[i];
                            self.autoSaveCollection('products', change.value.productId(), change.status);
                        }
                    }
                }, null, "arrayChange");
}  

            // Create variables for ListEditorApiUrls
            // Create loading function for Valid Values

            self.loadAssignedToValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/Person/CustomList?Fields=PersonId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.assignedToValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for AssignedTo: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.loadReportedByValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/Person/CustomList?Fields=PersonId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.reportedByValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for ReportedBy: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.loadCaseProductsValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/CaseProduct/CustomList?Fields=CaseProductId,CaseProductId", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.caseProductsValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for CaseProducts: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.loadDevTeamAssignedValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/DevTeam/CustomList?Fields=DevTeamId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.devTeamAssignedValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for DevTeamAssigned: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.showAssignedToEditor = function(callback: any) {
                if (!self.assignedTo()) {
                    self.assignedTo(new Person());
                }
                self.assignedTo().showEditor(callback)
            };
            self.showReportedByEditor = function(callback: any) {
                if (!self.reportedBy()) {
                    self.reportedBy(new Person());
                }
                self.reportedBy().showEditor(callback)
            };

            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                var obj;
                // See if self.assignedTo needs to be loaded.
                if (self.assignedTo() == null && self.assignedToId() != null){
                    loadingCount++;
                    obj = new Person();
                    obj.load(self.assignedToId(), function() {
                        loadingCount--;
                        self.assignedTo(obj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                // See if self.reportedBy needs to be loaded.
                if (self.reportedBy() == null && self.reportedById() != null){
                    loadingCount++;
                    obj = new Person();
                    obj.load(self.reportedById(), function() {
                        loadingCount--;
                        self.reportedBy(obj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                // See if self.devTeamAssigned needs to be loaded.
                if (self.devTeamAssigned() == null && self.devTeamAssignedId() != null){
                    loadingCount++;
                    obj = new DevTeam();
                    obj.load(self.devTeamAssignedId(), function() {
                        loadingCount--;
                        self.devTeamAssigned(obj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                if (loadingCount == 0 && $.isFunction(callback)){
                    callback();
                }
            };



            // Load all the valid values in parallel.
            self.loadValidValues = function(callback) {
                self.loadingValidValues = 0;
                self.loadAssignedToValidValues(callback);
                self.loadReportedByValidValues(callback);
                self.loadCaseProductsValidValues(callback);
                self.loadDevTeamAssignedValidValues(callback);
            };

            // Enumeration Lookups.
            self.statusText = ko.computed(function() {
                for(var i=0;i < self.statusValues.length; i++){
                    if (self.statusValues[i].id == self.status()){
                        return self.statusValues[i].value;
                    }
                }
            });


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





    export namespace Case {
        export enum StatusEnum {
            Open = 0,
            InProgress = 1,
            Resolved = 2,
            ClosedNoSolution = 3,
            Cancelled = 4,
        };

        // Classes for use in method calls to support data binding for input for arguments
    }
}
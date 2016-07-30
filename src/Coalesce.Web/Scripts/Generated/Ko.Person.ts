/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: Person
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2016

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {
    export var areaUrl = areaUrl || ((true) ? baseUrl : baseUrl + '/');

	export class Person
    {
        private loadingCount: number = 0;  // Stack for number of times loading has been called.
        private saveTimeout: number = 0;   // Stores the return value of setInterval for automatic save delays.
        // Callbacks to call after a delete.
        public deleteCallbacks: { ( myself: Person ): void; } [] = [];
        // Callbacks to call after a save.
        public saveCallbacks: { ( myself: Person ): void; } [] = [];    
        private loadingValidValues: number = 0;

        // String that defines what data should be included with the returned object.
        public includes = null;
        // If true, the busy indicator is shown when loading.
        public showBusyWhenSaving = false;  // If true a busy indicator shows when saving.

        // Parent of this object.
        public parent = null;
        // Collection that this object is a part of.
        public parentCollection = null;
        // ID of the object.
        public myId: any = 0;

        // Dirty Flag
        public isDirty: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for the page
        public errorMessage: KnockoutObservable<string> = ko.observable(null);
        // ValidationIssues returned from database when trying to persist data
        public validationIssues: any = ko.observableArray([]);
        // If this is true, all changes will be saved automatically.
        public isSavingAutomatically = true;
        // Flag to use to determine if this item is shown. Only for convenience.
        public isVisible: KnockoutObservable<boolean> = ko.observable(false);
        // Flag to use to determine if this item is expanded. Only for convenience.
        public isExpanded: KnockoutObservable<boolean> = ko.observable(false);
        // Flag to use to determine if this item is selected. Only for convenience.
        public isSelected: KnockoutObservable<boolean> = ko.observable(false);
        // Flag to use to determine if this item is checked. Only for convenience.
        public isChecked: KnockoutObservable<boolean> = ko.observable(false);
        // Alternates the isExpanded flag. Use with a click binding for a button.
        public changeIsExpanded: (value?: boolean) => void;
        // Flag to use to determine if this item is being edited. Only for convenience.
        public isEditing = ko.observable(false);
        // Alternates the isEditing flag. Use with a click binding for a button.
        public changeIsEditing: (value?: boolean) => void;
        // List of errors found during validation.
        public errors: any = ko.observableArray([]);
        // List of warnings found during validation. These allow a save.
        public warnings: any = ko.observableArray([]);
        // Custom Field that can be used via scripts. This allows for setting observables via scripts and using them without modifying the ViewModel
        public customField1: KnockoutObservable<any> = ko.observable();
        // Custom Field 2 that can be used via scripts. This allows for setting observables via scripts and using them without modifying the ViewModel
        public customField2: KnockoutObservable<any> = ko.observable();
        // Custom Field 3 that can be used via scripts. This allows for setting observables via scripts and using them without modifying the ViewModel
        public customField3: KnockoutObservable<any> = ko.observable();


        // True if the object is currently saving.
        public isSaving: KnockoutObservable<boolean> = ko.observable(false);
        // Internal count of child objects that are saving.
        public savingChildCount: KnockoutObservable<number> = ko.observable(0);
        // True if the object or any of its children are currently saving.
        public isSavingWithChildren: () => boolean;
        // Internally used member to count the number of saving children.
        public savingChildChange: (isSaving: boolean) => void;

        // Set this false when editing a field that saves periodically while the user is typing. 
        // By default(null), isDataFromSaveLoadedComputed will check the parent's value. 
        // If the topmost parent is null, the value is true. Otherwise the first set value will be returned.
        public isDataFromSaveLoaded: boolean = null;
        // Used internally to determine if the data from a save operation should be loaded.
        public isDataFromSaveLoadedComputed: () => boolean;
        public isValid: () => boolean;
    
        // Observables
        // ID for the person object.
        public personId: KnockoutObservable<number> = ko.observable(null);
        // Title of the person, Mr. Mrs, etc.
        public title: KnockoutObservable<number> = ko.observable(null);
        // Text value for enumeration Title
        // Title of the person, Mr. Mrs, etc.
        public titleText: KnockoutComputed<string> = ko.computed<string>(() => "");
        // First name of the person.
        public firstName: KnockoutObservable<string> = ko.observable(null);
        // Last name of the person
        public lastName: KnockoutObservable<string> = ko.observable(null);
        // Email address of the person
        public email: KnockoutObservable<string> = ko.observable(null);
        // Genetic Gender of the person.
        public gender: KnockoutObservable<number> = ko.observable(null);
        // Text value for enumeration Gender
        // Genetic Gender of the person.
        public genderText: KnockoutComputed<string> = ko.computed<string>(() => "");
        // List of cases assigned to the person
        public casesAssigned: KnockoutObservableArray<any> = ko.observableArray([]);
        // List of cases reported by the person.
        public casesReported: KnockoutObservableArray<any> = ko.observableArray([]);
        public birthDate: KnockoutObservable<any> = ko.observable(null);
        public lastBath: KnockoutObservable<any> = ko.observable(null);
        public nextUpgrade: KnockoutObservable<any> = ko.observable(null);
        public personStatsId: KnockoutObservable<number> = ko.observable(null);
        public personStats: KnockoutObservable<ViewModels.PersonStats> = ko.observable(null);
        public timeZone: KnockoutObservable<any> = ko.observable(null);
        // Calculated name of the person. eg., Mr. Michael Stokesbary.
        public name: KnockoutObservable<string> = ko.observable(null);
        // Company ID this person is employed by
        public companyId: KnockoutObservable<number> = ko.observable(null);
        // Company loaded from the Company ID
        public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);

       
        // True if the object is loading.
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        // URL to a stock editor for this object.
        public editUrl: () => string;
        // Create computeds for display for objects
        public personStatsText: () => string;
        // Company loaded from the Company ID
        public companyText: () => string;
        // Loads this object from a data transfer object received from the server.
        public loadFromDto: (data: any) => void;
        // Saves this object into a data transfer object to send to the server.
        public saveToDto: () => any;
        // Saves the object to the server and then calls the callback.
        public save: (callback?: any) => void;
        // Validates the model.
        public validate: () => boolean;
        // Loads the object from the server based on the id specified. Once complete calls the callback.
        public load: (id: any, callback?: any) => void
        // Reloads the object from the server.
        public reload: (callback?: any) => void;
        // Deletes the object after a user confirmation. Bind this to delete buttons.
        public deleteItemWithConfirmation: (callback?: any, message?: string) => void;
        // Deletes the object without confirmation.
        public deleteItem: (callback?: any) => void;
        
        // Add object to casesAssigned
        public addToCasesAssigned: () => Case;
        // List Object model for CasesAssigned. Allows for loading subsets of data.
        public casesAssignedList: () => ListViewModels.CaseList;
        // Add object to casesReported
        public addToCasesReported: () => Case;
        // List Object model for CasesReported. Allows for loading subsets of data.
        public casesReportedList: () => ListViewModels.CaseList;
        
        // Saves a many-to-many collection change. This is done automatically and doesn't need to be called.
        public saveCollection: (propertyName, childId, operation)  => void;
        // Callback to be called when this item is deleted.
        public onDelete: (fn: any) => void;
        // Callback to be called when a save is done.
        public onSave: (fn: any) => void;
        // If true, changes to the object are saved automatically. If false, save() needs to be called.
        public autoSave: () => void;
        // If true, many-to-many collections are automatically saved.
        public autoSaveCollection: (property: string, id: any, changeStatus: string) => void;
        // List of cases assigned to the person
        public CasesAssignedListUrl: () => void; 
        // List of cases reported by the person.
        public CasesReportedListUrl: () => void; 
                public personStatsValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadPersonStatsValidValues: (callback?: any) => void;
        // Company loaded from the Company ID
        public companyValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCompanyValidValues: (callback?: any) => void;
        // Pops up a stock editor for this object.
        public showEditor: () => void;
        public showPersonStatsEditor: () => void;
        public showCompanyEditor: () => void;
        // Loads collections of valid values. May be removed.
        public loadValidValues: (callback?: any) => void;
        // Loads any children that have an ID but have not been loaded. 
        // This is useful when creating an object that has a parent object and the ID is set on the new child.
        public loadChildren: (callback?: any) => void;
        // Selects this item and deselects other items in the parentCollection.
        public selectSingle: () => boolean;
        public isSelectedToggle: () => boolean;


        public titleValues: EnumValue[] = [ 
            { id: 0, value: 'Mr' },
            { id: 1, value: 'Ms' },
            { id: 2, value: 'Mrs' },
            { id: 4, value: 'Miss' },
        ];
        public genderValues: EnumValue[] = [ 
            { id: 0, value: 'Non Specified' },
            { id: 1, value: 'Male' },
            { id: 2, value: 'Female' },
        ];

        // Call server method (Rename)
        // Adds the text to the first name.
        public rename: (addition: String, callback?: any) => void;
        // Result of server method (Rename)
        public renameResult: KnockoutObservable<any> = ko.observable();
        // True while the server method (Rename) is being called
        public renameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (Rename) if it fails.
        public renameMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (Rename) was successful.
        public renameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (Rename)
        public renameUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (Rename)
        public renameModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        public renameWithArgs: (args?: Person.RenameArgs, callback?: any) => void;
        // Object that can be easily bound to fields to allow data entry for the method
        public renameArgs = new Person.RenameArgs(); 
        
        // Call server method (ChangeSpacesToDashesInName)
        // Removes spaces from the name and puts in dashes
        public changeSpacesToDashesInName: (callback?: any) => void;
        // Result of server method (ChangeSpacesToDashesInName)
        public changeSpacesToDashesInNameResult: KnockoutObservable<any> = ko.observable();
        // True while the server method (ChangeSpacesToDashesInName) is being called
        public changeSpacesToDashesInNameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (ChangeSpacesToDashesInName) if it fails.
        public changeSpacesToDashesInNameMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (ChangeSpacesToDashesInName) was successful.
        public changeSpacesToDashesInNameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (ChangeSpacesToDashesInName)
        public changeSpacesToDashesInNameUi: (callback?: any) => void;
        // Presents a modal with input boxes to call the server method (ChangeSpacesToDashesInName)
        public changeSpacesToDashesInNameModal: (callback?: any) => void;
        // Variable for method arguments to allow for easy binding
        
        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: Person) {};

        // This method gets called after loading data (loadFromDto), and allows setting DTO specific properties before binding happens.
        public dtoInit(myself: Person) {};

        constructor(newItem?: any, parent?: any){
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
			self.firstName = self.firstName.extend({ minLength: 2 });
			self.lastName = self.lastName.extend({ minLength: 3 });
			self.birthDate = self.birthDate.extend({ moment: { unix: true } });
			self.lastBath = self.lastBath.extend({ moment: { unix: true } });
			self.nextUpgrade = self.nextUpgrade.extend({ moment: { unix: true } });
            
            self.errors = ko.validation.group([
                self.personId,
                self.title,
                self.firstName,
                self.lastName,
                self.email,
                self.gender,
                self.casesAssigned,
                self.casesReported,
                self.birthDate,
                self.lastBath,
                self.nextUpgrade,
                self.personStatsId,
                self.personStats,
                self.timeZone,
                self.name,
                self.companyId,
                self.company,
            ]);
            self.warnings = ko.validation.group([
            ]);

            self.changeIsExpanded = function(value?: boolean) { // Call this with the edit button.
                if (value !== true || value !== false) self.isExpanded(!self.isExpanded());
                else self.isExpanded(value === true); // Force boolean
            };
            self.changeIsEditing = function (value){  // Call this with the edit button.
                if (value !== true || value !== false) self.isEditing(!self.isEditing());
                else self.isEditing(value === true);  // Force boolean
            };

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return areaUrl + "Person/CreateEdit?id=" + self.personId();
            });

            // Create computeds for display for objects
			self.personStatsText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.personStats() && self.personStats().personStatsId()) {
					return self.personStats().personStatsId().toString();
				} else {
					return "None";
				}
			});
			self.companyText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.company() && self.company().altName()) {
					return self.company().altName().toString();
				} else {
					return "None";
				}
			});


            // Load the object
			self.loadFromDto = function(data: any) {
				if (!data) return;
				self.isLoading(true);
				// Set the ID 
				self.myId = data.personId;
				// Load the lists of other objects
                if (data.CasesAssigned !== null) {
					// Merge the incoming array
					RebuildArray(self.casesAssigned, data.casesAssigned, 'caseKey', Case, self);
				} 
                if (data.CasesReported !== null) {
					// Merge the incoming array
					RebuildArray(self.casesReported, data.casesReported, 'caseKey', Case, self);
				} 
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.
				if (!data.personStats) { 
					if (data.personStatsId != self.personStatsId()) {
                        self.personStats(null);
                    }
                }else if (!self.personStats()){
					self.personStats(new PersonStats(data.personStats, self));
				}else{
					self.personStats().loadFromDto(data.personStats);
				}
				if (!data.company) { 
					if (data.companyId != self.companyId()) {
                        self.company(null);
                    }
                }else if (!self.company()){
					self.company(new Company(data.company, self));
				}else{
					self.company().loadFromDto(data.company);
				}

				// The rest of the objects are loaded now.
				self.personId(data.personId);
				self.title(data.title);
				self.firstName(data.firstName);
				self.lastName(data.lastName);
				self.email(data.email);
				self.gender(data.gender);
                if (data.birthDate == null) self.birthDate(null);
				else if (self.birthDate() == null || self.birthDate() == false || !self.birthDate().isSame(moment(data.birthDate))){
				    self.birthDate(moment(data.birthDate));
				}
                if (data.lastBath == null) self.lastBath(null);
				else if (self.lastBath() == null || self.lastBath() == false || !self.lastBath().isSame(moment(data.lastBath))){
				    self.lastBath(moment(data.lastBath));
				}
                if (data.nextUpgrade == null) self.nextUpgrade(null);
				else if (self.nextUpgrade() == null || self.nextUpgrade() == false || !self.nextUpgrade().isSame(moment(data.nextUpgrade))){
				    self.nextUpgrade(moment(data.nextUpgrade));
				}
				self.personStatsId(data.personStatsId);
				self.timeZone(data.timeZone);
				self.name(data.name);
				self.companyId(data.companyId);
                self.originalData(data);

                // Add simple observables for any DTO properties that aren't on our view model
                for (var key in data) {
                    if (data.hasOwnProperty(key) && !self.hasOwnProperty(key)) {
                        self[key] = ko.observable(data[key]);
                    }
                }

                // Call an init function that allows for setting DTO specific properties before binding.
                if ($.isFunction(self.dtoInit)){
                    self.dtoInit(self);
                }

				self.isLoading(false);
				self.isDirty(false);
                self.validate();
			};

    	    // Save the object into a DTO
			self.saveToDto = function() {
				var dto: any = {};
				dto.personId = self.personId();

    	        dto.title = self.title();
    	        dto.firstName = self.firstName();
    	        dto.lastName = self.lastName();
    	        dto.email = self.email();
    	        dto.gender = self.gender();
                if (!self.birthDate()) dto.BirthDate = null;
				else dto.birthDate = self.birthDate().format('YYYY-MM-DDTHH:mm:ss');
                if (!self.lastBath()) dto.LastBath = null;
				else dto.lastBath = self.lastBath().format('YYYY-MM-DDTHH:mm:ss');
				if (!self.nextUpgrade()) dto.NextUpgrade = null;
				else dto.nextUpgrade = self.nextUpgrade().format('YYYY-MM-DDTHH:mm:ssZZ');
				dto.personStatsId = self.personStatsId();
				if (!dto.personStatsId && self.personStats()) {
				    dto.personStatsId = self.personStats().personStatsId();
				}
    	        dto.timeZone = self.timeZone();
				dto.companyId = self.companyId();
				if (!dto.companyId && self.company()) {
				    dto.companyId = self.company().companyId();
				}

				return dto;
			}

			self.save = function(callback?: any) {
				if (!self.isLoading()) {
					if (self.validate()) {
						if (self.showBusyWhenSaving) intellitect.utilities.showBusy();
						self.isSaving(true);
                        $.ajax({ method: "POST", url: areaUrl + "api/Person/Save?includes=" + self.includes, data: self.saveToDto(), xhrFields: { withCredentials: true } })
						.done(function(data) {
							self.isDirty(false);
							self.errorMessage('');
                            if (self.isDataFromSaveLoadedComputed()) {
								self.loadFromDto(data.object);
                            }
							// The object is now saved. Call any callback.
							for (var i in self.saveCallbacks) {
								self.saveCallbacks[i](self);
							}
						})
						.fail(function(xhr) {
                            var errorMsg = "Unknown Error";
                            var validationIssues = [];
                            if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                            if (xhr.responseJSON && xhr.responseJSON.validationIssues) validationIssues = xhr.responseJSON.validationIssues;
                            self.errorMessage(errorMsg);
                            self.validationIssues(validationIssues);

							alert("Could not save the item: " + errorMsg);
						})
						.always(function() {
							self.isSaving(false);
							if ($.isFunction(callback)) {
								callback();
							}
							if (self.showBusyWhenSaving) intellitect.utilities.hideBusy();
						});
					}
				}
			}

			// Assign this function to add validation that prevents saving  by returning false.
			// Return true to continue to save.
			self.validate = function() { 
                self.errors.showAllMessages();
                self.warnings.showAllMessages();
                return self.errors().length == 0;
            };

			// Loads an item.
			self.load = function(id: any, callback?) {
                if (!id) {
                    id = self.personId();
                }
                if (id) {
                    self.isLoading(true);
                    intellitect.utilities.showBusy();
                    $.ajax({ method: "GET", url: areaUrl + "api/Person/Get/" + id + '?includes=' + self.includes, xhrFields: { withCredentials: true } })
                        .done(function(data) {
                            self.loadFromDto(data);
                            if ($.isFunction(callback)) callback(self);
                        })
                        .fail(function() {
                            alert("Could not get Person with id = " + id);
                        })
                        .always(function() {
                            intellitect.utilities.hideBusy();
                            self.isLoading(false);
                        });
                }
            };

            self.reload = function(callback) {
                self.load(self.personId(), callback);
            };

            // Deletes the object after a confirmation box.
            self.deleteItemWithConfirmation = function(callback, message) {
                if (typeof message != 'string') {
                    message = "Delete this item?";
                }
                if (confirm(message)) {
                    self.deleteItem(callback);
                }
            };

            // Deletes the object
            self.deleteItem = function(callback) {
                var currentId = self.personId();
                if (currentId){
                $.ajax({ method: "POST", url: areaUrl+ "api/Person/Delete/" + currentId, xhrFields: { withCredentials: true } })
                .done(function(data) {
                    if (data) {
                        self.errorMessage('');
                        // The object is now deleted. Call any callback.
                        for (var i in self.deleteCallbacks) {
                            self.deleteCallbacks[i](self);
                        }
                        // Remove it from the parent collection
                        if (self.parentCollection && self.parent) {
                            self.parent.isLoading(true);
                            self.parentCollection.splice(self.parentCollection().indexOf(self),1);
                            self.parent.isLoading(false);
                        }
                    } else {
                        self.errorMessage(data.message);
                    }
                })
                .fail(function() {
                    alert("Could not delete the item.");
                })
                .always(function() {
                    if ($.isFunction(callback)) {
                        callback(callback);
                    }
                });
                }else{
                    // No ID has been assigned yet, just remove it.
                    if (self.parentCollection && self.parent) {
                        self.parent.isLoading(true);
                        self.parentCollection.splice(self.parentCollection().indexOf(self),1);
                        self.parent.isLoading(false);
                    }
                    if ($.isFunction(callback)) {
                        callback(callback);
                    }
                }
            };

            // Sets isSelected(true) on this object and clears on the rest of the items in the parentCollection. Returns true to bubble additional click events.
            self.selectSingle = function () {
                if (self.parentCollection()) {
                    $.each(self.parentCollection(), function (i, obj) {
                        obj.isSelected(false);
                    });
                }
                self.isSelected(true);
                return true; // Allow other click events
            };

            // Toggles isSelected value. Returns true to bubble additional click events.
            self.isSelectedToggle = function() {
                self.isSelected(!self.isSelected());
                return true;
            }

            // Methods to add to child collections

            self.addToCasesAssigned = function() {
                var newItem = new Case();
                newItem.parent = self;
                newItem.parentCollection = self.casesAssigned;
                newItem.isExpanded(true);
                newItem.assignedToId(self.personId());
                self.casesAssigned.push(newItem);
                return newItem;
            }
            
            self.addToCasesReported = function() {
                var newItem = new Case();
                newItem.parent = self;
                newItem.parentCollection = self.casesReported;
                newItem.isExpanded(true);
                newItem.reportedById(self.personId());
                self.casesReported.push(newItem);
                return newItem;
            }
            
            // List Object model for CasesAssigned. Allows for loading subsets of data.
            var _casesAssignedList: ListViewModels.CaseList = null;
            self.casesAssignedList = function() {
                if (!_casesAssignedList){
                    _casesAssignedList = new ListViewModels.CaseList();
                    loadCasesAssignedList();
                    self.personId.subscribe(loadCasesAssignedList)
                }
                return _casesAssignedList;
            }
            function loadCasesAssignedList() {
                if (self.personId()){
                    _casesAssignedList.queryString = "AssignedToId=" + self.personId();
                    _casesAssignedList.load();
                }
            }
            // List Object model for CasesReported. Allows for loading subsets of data.
            var _casesReportedList: ListViewModels.CaseList = null;
            self.casesReportedList = function() {
                if (!_casesReportedList){
                    _casesReportedList = new ListViewModels.CaseList();
                    loadCasesReportedList();
                    self.personId.subscribe(loadCasesReportedList)
                }
                return _casesReportedList;
            }
            function loadCasesReportedList() {
                if (self.personId()){
                    _casesReportedList.queryString = "ReportedById=" + self.personId();
                    _casesReportedList.load();
                }
            }

            // Save a many-to-many collection
            self.saveCollection = function(propertyName, childId, operation) {
                var method = (operation === "added" ? "AddToCollection" : "RemoveFromCollection");
                var currentId = self.personId();
                $.ajax({ method: "POST", url: areaUrl + 'api/Person/' + method + '?id=' + currentId + '&propertyName=' + propertyName + '&childId=' + childId, xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.errorMessage('');
                    self.loadFromDto(data.object);
                    // The object is now saved. Call any callback.
                    for (var i in self.saveCallbacks) {
                        self.saveCallbacks[i](self);
                    }
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    var validationIssues = [];
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    if (xhr.responseJSON && xhr.responseJSON.validationIssues) errorMsg = xhr.responseJSON.validationIssues;
                    self.errorMessage(errorMsg);
                    self.validationIssues(validationIssues);

                    alert("Could not save the item: " + errorMsg);
                })
                .always(function() {
                    // Nothing here yet.
                });
            };

            // Call this function when the object is deleted.
            self.onDelete = function(fn) {
                if ($.isFunction(fn)) self.deleteCallbacks.push(fn);
            };
            self.onSave = function(fn) {
                if ($.isFunction(fn)) self.saveCallbacks.push(fn);
            };

            // Saves the object is autoSave is true.
            self.autoSave = function() {
                if (!self.isLoading()){
                    self.isDirty(true);
                    if (self.isSavingAutomatically) {
                        // Batch saves.
                        if (!self.saveTimeout) {
                            self.saveTimeout = setTimeout(function() {
                                self.saveTimeout = 0;
                                // If we have a save in progress, wait...
                                if (self.isSaving()) {
                                    self.autoSave();
                                }else{
                                    self.save();
                                }
                            }, saveTimeoutInMs);
                        }
                    }
                }
            }

            // Saves the object is autoSave is true.
            self.autoSaveCollection = function(property, id, changeStatus) {
                if (!self.isLoading()) {
                    // TODO: Eventually Batch saves for many-to-many collections.
                    if (changeStatus === 'added') {
                        self.saveCollection(property, id, "added");
                    }else if (changeStatus === 'deleted') {
                        self.saveCollection(property, id, "deleted");
                    }
                }
            }

            // Save on changes
            function setupSubscriptions() {
            self.title.subscribe(self.autoSave);
            self.firstName.subscribe(self.autoSave);
            self.lastName.subscribe(self.autoSave);
            self.email.subscribe(self.autoSave);
            self.gender.subscribe(self.autoSave);
            self.casesAssigned.subscribe(self.autoSave);
            self.casesReported.subscribe(self.autoSave);
            self.birthDate.subscribe(self.autoSave);
            self.lastBath.subscribe(self.autoSave);
            self.nextUpgrade.subscribe(self.autoSave);
            self.personStatsId.subscribe(self.autoSave);
            self.timeZone.subscribe(self.autoSave);
            self.companyId.subscribe(self.autoSave);
            self.company.subscribe(self.autoSave);
                        }

            // Create variables for ListEditorApiUrls
            self.CasesAssignedListUrl = ko.computed({
                read: function() {
                         return areaUrl + 'Case/table?assignedToId=' + self.personId();
                },
                deferEvaluation: true
            });
            self.CasesReportedListUrl = ko.computed({
                read: function() {
                         return areaUrl + 'Case/table?reportedById=' + self.personId();
                },
                deferEvaluation: true
            });
            // Create loading function for Valid Values

            self.loadPersonStatsValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: areaUrl + "api/PersonStats/CustomList?Fields=PersonStatsId,PersonStatsId", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.personStatsValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    alert("Could not get Valid Values for PersonStats: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.loadCompanyValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: areaUrl + "api/Company/CustomList?Fields=CompanyId,AltName", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.companyValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    alert("Could not get Valid Values for Company: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            // Supply methods to pop up a model editor
            self.showEditor = function(){
                // Close any existing modal
                $('#modal-dialog').modal('hide');
                // Get new modal content
                intellitect.utilities.showBusy();
                $.ajax({ method: "GET", url: areaUrl + 'Person/EditorHtml', data: {simple: true}, xhrFields: { withCredentials: true } })
                .done(function(data){
                    // Add to DOM
                    intellitect.webApi.setupModal('Edit Person', data, true, false);
                    // Data bind
                    var lastValue = self.isSavingAutomatically;
                    self.isSavingAutomatically = false;
                    ko.applyBindings(self, document.getElementById("modal-dialog"));
                    self.isSavingAutomatically = lastValue;
                    // Show the dialog
                    $('#modal-dialog').modal('show');
                })
                .always(function() {
                    intellitect.utilities.hideBusy();
                });
            }

            self.showCompanyEditor = function() {
                if (!self.company()) {
                    self.company(new Company());
                }
                self.company().showEditor()
            };

            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                var obj;
                // See if self.company needs to be loaded.
                if (self.company() == null && self.companyId() != null){
                    loadingCount++;
                    obj = new Company();
                    obj.load(self.companyId(), function() {
                        loadingCount--;
                        self.company(obj);
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
                self.loadPersonStatsValidValues(callback);
                self.loadCompanyValidValues(callback);
            };

            // Enumeration Lookups.
            self.titleText = ko.computed(function() {
                for(var i=0;i < self.titleValues.length; i++){
                    if (self.titleValues[i].id == self.title()){
                        return self.titleValues[i].value;
                    }
                }
            });
            self.genderText = ko.computed(function() {
                for(var i=0;i < self.genderValues.length; i++){
                    if (self.genderValues[i].id == self.gender()){
                        return self.genderValues[i].value;
                    }
                }
            });

            // Code to handle saving flags.
            // Returns true if this object or any of its children is saving.
            self.isSavingWithChildren = ko.computed(function() {
                if (self.isSaving()) return true;
                if (self.savingChildCount() > 0 ) return true;
                return false;
            });
            // Handles setting the parent savingChildChange
            self.isSaving.subscribe(function(newValue: boolean){
                if (self.parent && $.isFunction(self.parent.savingChildChange)){
                    self.parent.savingChildChange(newValue);
                }
            })
            // Handle children that are saving.
            self.savingChildChange = function(isSaving: boolean){
                if (isSaving) self.savingChildCount(self.savingChildCount() + 1);
                else self.savingChildCount(self.savingChildCount() - 1);
                if (self.parent && $.isFunction(self.parent.savingChildChange)){
                    self.parent.savingChildChange(isSaving);
                }
            }

            // Code to handle isDataFromSaveLoaded
            self.isDataFromSaveLoadedComputed = function() {
                if (self.isDataFromSaveLoaded === false) return false;
                if (self.isDataFromSaveLoaded === true) return true;
                if (self.parent && $.isFunction(self.parent.isDataFromSaveLoadedComputed)){
                    return self.parent.isDataFromSaveLoadedComputed();
                }
                return true;
            }


            // Method Implementations

            self.rename = function(addition: String, callback?: any, reload: Boolean = true){
                self.renameIsLoading(true);
                $.ajax({ method: "POST",
                         url: areaUrl + "api/Person/Rename",
                         data: {
                        id: self.myId, 
                        addition: addition
                    },
                         xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isDirty(false);
                    self.renameMessage('');
                    self.renameWasSuccessful(true);
                    self.renameResult(data.object);
                    // The return type is the type of the object, load it.
                    self.loadFromDto(data.object)
                    if ($.isFunction(callback)) {
                        callback();
                    }
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.renameWasSuccessful(false);
                    self.renameMessage(errorMsg);

                    alert("Could not call method rename: " + errorMsg);
                })
                .always(function() {
                    self.renameIsLoading(false);
                });
            }
            
            self.renameUi = function(callback?: any) {
                var addition: String = prompt('Addition');
                self.rename(addition, callback);
            }
            self.renameModal = function(callback?: any) {
                $('#method-Rename').modal();
                $('#method-Rename').on('shown.bs.modal', function() {
                    $('#method-Rename .btn-ok').click(function()
                    {
                        self.renameWithArgs(null, callback);
                        $('#method-Rename').modal('hide');
                    });
                });
            }
            self.renameWithArgs = function(args?: Person.RenameArgs, callback?: any) {
                if (!args) args = self.renameArgs;
                self.rename(args.addition(), callback);
            }

            self.changeSpacesToDashesInName = function(callback?: any, reload: Boolean = true){
                self.changeSpacesToDashesInNameIsLoading(true);
                $.ajax({ method: "POST",
                         url: areaUrl + "api/Person/ChangeSpacesToDashesInName",
                         data: {
                        id: self.myId
                    },
                         xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isDirty(false);
                    self.changeSpacesToDashesInNameMessage('');
                    self.changeSpacesToDashesInNameWasSuccessful(true);
                    self.changeSpacesToDashesInNameResult(data.object);
                    if (reload) {
                      self.reload(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.changeSpacesToDashesInNameWasSuccessful(false);
                    self.changeSpacesToDashesInNameMessage(errorMsg);

                    alert("Could not call method changeSpacesToDashesInName: " + errorMsg);
                })
                .always(function() {
                    self.changeSpacesToDashesInNameIsLoading(false);
                });
            }
            
            self.changeSpacesToDashesInNameUi = function(callback?: any) {
                self.changeSpacesToDashesInName(callback);
            }
            self.changeSpacesToDashesInNameModal = function(callback?: any) {
                    self.changeSpacesToDashesInNameUi(callback);
            }


            // This stuff needs to be done after everything else is set up.
            // Complex Type Observables

            // Make sure everything is defined before we call this.
            setupSubscriptions();

            if (newItem) {
                if ($.isNumeric(newItem)) self.load(newItem);
                else self.loadFromDto(newItem);
            }



        }
    }





    export namespace Person {
        export enum TitleEnum {
            Mr = 0,
            Ms = 1,
            Mrs = 2,
            Miss = 4,
        };
        export enum GenderEnum {
            NonSpecified = 0,
            Male = 1,
            Female = 2,
        };

        // Classes for use in method calls to support data binding for input for arguments
        export class RenameArgs {
            public addition: KnockoutObservable<string> = ko.observable(null);
        }
    }
}
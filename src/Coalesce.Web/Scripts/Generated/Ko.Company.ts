/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: Company
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2016

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {
    export var areaUrl = areaUrl || ((true) ? baseUrl : baseUrl + '/');

	export class Company
    {
        private loadingCount: number = 0;  // Stack for number of times loading has been called.
        private saveTimeout: number = 0;   // Stores the return value of setInterval for automatic save delays.
        // Callbacks to call after a delete.
        public deleteCallbacks: { ( myself: Company ): void; } [] = [];
        // Callbacks to call after a save.
        public saveCallbacks: { ( myself: Company ): void; } [] = [];    
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
        public companyId: KnockoutObservable<number> = ko.observable(null);
        public name: KnockoutObservable<string> = ko.observable(null);
        public address1: KnockoutObservable<string> = ko.observable(null);
        public address2: KnockoutObservable<string> = ko.observable(null);
        public city: KnockoutObservable<string> = ko.observable(null);
        public state: KnockoutObservable<string> = ko.observable(null);
        public zipCode: KnockoutObservable<string> = ko.observable(null);
        public employees: KnockoutObservableArray<ViewModels.Person> = ko.observableArray([]);
        public altName: KnockoutObservable<string> = ko.observable(null);

       
        // True if the object is loading.
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        // URL to a stock editor for this object.
        public editUrl: () => string;
        // Create computeds for display for objects
        // Loads this object from a data transfer object received from the server.
        // Force: Will override the check against isLoading that is done to prevent recursion.
        // AllowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
        public loadFromDto: (data: any, force?: boolean, allowCollectionDeletes?: boolean) => void;
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
        
        public addToEmployees: () => Person;
        // List Object model for Employees. Allows for loading subsets of data.
        public employeesList: () => ListViewModels.PersonList;
        
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
        public EmployeesListUrl: () => void; 
                // Pops up a stock editor for this object.
        public showEditor: () => void;
        // Loads collections of valid values. May be removed.
        public loadValidValues: (callback?: any) => void;
        // Loads any children that have an ID but have not been loaded. 
        // This is useful when creating an object that has a parent object and the ID is set on the new child.
        public loadChildren: (callback?: any) => void;
        // Selects this item and deselects other items in the parentCollection.
        public selectSingle: () => boolean;
        public isSelectedToggle: () => boolean;



        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: Company) {};

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
                return areaUrl + "Company/CreateEdit?id=" + self.companyId();
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

			self.save = function(callback?: any) {
				if (!self.isLoading()) {
					if (self.validate()) {
						if (self.showBusyWhenSaving) intellitect.utilities.showBusy();
						self.isSaving(true);
                        $.ajax({ method: "POST", url: areaUrl + "api/Company/Save?includes=" + self.includes, data: self.saveToDto(), xhrFields: { withCredentials: true } })
						.done(function(data) {
							self.isDirty(false);
							self.errorMessage('');
                            if (self.isDataFromSaveLoadedComputed()) {
								self.loadFromDto(data.object, true);
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
                            // If an object was returned, load that object.
                            if (xhr.responseJSON && xhr.responseJSON.object){
                                self.loadFromDto(xhr.responseJSON.object, true);
                            }
                            // TODO: allow for turning this off
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
                    else
                    {
                        // If validation fails, we still want to try and load any child objects which may have just been set.
                        // Normally, we get these from the result of the save.
                        self.loadChildren();
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
                    id = self.companyId();
                }
                if (id) {
                    self.isLoading(true);
                    intellitect.utilities.showBusy();
                    $.ajax({ method: "GET", url: areaUrl + "api/Company/Get/" + id + '?includes=' + self.includes, xhrFields: { withCredentials: true } })
                        .done(function(data) {
                            self.loadFromDto(data, true);
                            if ($.isFunction(callback)) callback(self);
                        })
                        .fail(function() {
                            alert("Could not get Company with id = " + id);
                        })
                        .always(function() {
                            intellitect.utilities.hideBusy();
                            self.isLoading(false);
                        });
                }
            };

            self.reload = function(callback) {
                self.load(self.companyId(), callback);
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
                var currentId = self.companyId();
                if (currentId){
                $.ajax({ method: "POST", url: areaUrl+ "api/Company/Delete/" + currentId, xhrFields: { withCredentials: true } })
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
            self.employeesList = function() {
                if (!_employeesList){
                    _employeesList = new ListViewModels.PersonList();
                    loadEmployeesList();
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

            // Save a many-to-many collection
            self.saveCollection = function(propertyName, childId, operation) {
                var method = (operation === "added" ? "AddToCollection" : "RemoveFromCollection");
                var currentId = self.companyId();
                $.ajax({ method: "POST", url: areaUrl + 'api/Company/' + method + '?id=' + currentId + '&propertyName=' + propertyName + '&childId=' + childId, xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.errorMessage('');
                    self.loadFromDto(data.object, true);
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
                         return areaUrl + 'Person/table?companyId=' + self.companyId();
                },
                deferEvaluation: true
            });
            // Create loading function for Valid Values

            // Supply methods to pop up a model editor
            self.showEditor = function(){
                // Close any existing modal
                $('#modal-dialog').modal('hide');
                // Get new modal content
                intellitect.utilities.showBusy();
                $.ajax({ method: "GET", url: areaUrl + 'Company/EditorHtml', data: {simple: true}, xhrFields: { withCredentials: true } })
                .done(function(data){
                    // Add to DOM
                    intellitect.webApi.setupModal('Edit Company', data, true, false);
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
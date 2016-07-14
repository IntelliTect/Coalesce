/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Intellitect/intellitect.utilities.ts" />
/// <reference path="../Intellitect/intellitect.ko.utilities.ts" />
/// <reference path="../Intellitect/intellitect.ko.base.ts" />



// Knockout View Model for: Case
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2016

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {
    export var areaUrl = areaUrl || ((true) ? baseUrl : baseUrl + '/');

	export class Case
    {
        private loadingCount: number = 0;  // Stack for number of times loading has been called.
        private saveTimeout: number = 0;   // Stores the return value of setInterval for automatic save delays.
        // Callbacks to call after a delete.
        public deleteCallbacks: { ( myself: Case ): void; } [] = [];
        // Callbacks to call after a save.
        public saveCallbacks: { ( myself: Case ): void; } [] = [];    
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
        // The Primary key for the Case object
        public caseKey: KnockoutObservable<number> = ko.observable(null);
        public title: KnockoutObservable<string> = ko.observable(null);
        public description: KnockoutObservable<string> = ko.observable(null);
        public openedAt: KnockoutObservable<moment.Moment> = ko.observable(moment());
        public assignedToId: KnockoutObservable<number> = ko.observable(null);
        public assignedTo: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        public reportedById: KnockoutObservable<number> = ko.observable(null);
        public reportedBy: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        public attachment: KnockoutObservableArray<number> = ko.observableArray([]);
        public severity: KnockoutObservable<string> = ko.observable(null);
        public status: KnockoutObservable<number> = ko.observable(null);
        // Text value for enumeration Status
        public statusText: KnockoutComputed<string> = ko.computed<string>(() => "");
        public caseProducts: KnockoutObservableArray<any> = ko.observableArray([]);
        public products: KnockoutObservableArray<ViewModels.Product> = ko.observableArray([]);  // Many to Many Collection
        public devTeamAssignedId: KnockoutObservable<number> = ko.observable(null);
        public devTeamAssigned: KnockoutObservable<ViewModels.DevTeam> = ko.observable(null);

       
        // True if the object is loading.
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        // URL to a stock editor for this object.
        public editUrl: () => string;
        // Create computeds for display for objects
        public assignedToText: () => string;
        public reportedByText: () => string;
        public devTeamAssignedText: () => string;
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
        public showEditor: () => void;
        public showAssignedToEditor: () => void;
        public showReportedByEditor: () => void;
        public showDevTeamAssignedEditor: () => void;
        // Loads collections of valid values. May be removed.
        public loadValidValues: (callback?: any) => void;
        // Loads any children that have an ID but have not been loaded. 
        // This is useful when creating an object that has a parent object and the ID is set on the new child.
        public loadChildren: (callback?: any) => void;
        // Selects this item and deselects other items in the parentCollection.
        public selectSingle: () => boolean;
        public isSelectedToggle: () => boolean;


        public statusValues: EnumValue[] = [ 
            { id: 0, value: 'Open' },
            { id: 1, value: 'In Progress' },
            { id: 2, value: 'Resolved' },
            { id: 3, value: 'Closed No Solution' },
            { id: 4, value: 'Cancelled' },
        ];




        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: Case) {};


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
                return areaUrl + "Case/CreateEdit?id=" + self.caseKey();
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


            // Load the object
			self.loadFromDto = function(data: any) {
				if (!data) return;
				self.isLoading(true);
				// Set the ID 
				self.myId = data.caseKey;
				// Load the lists of other objects
                if (data.CaseProducts !== null) {
					// Merge the incoming array
					RebuildArray(self.caseProducts, data.caseProducts, 'caseProductId', CaseProduct, self);
                    // Add many-to-many collection
                    var objs = [];
                    $.each(data.caseProducts, function(index, item) {
                        if (item.product){
                            objs.push(item.product);
                        }
                    });
					RebuildArray(self.products, objs, 'ProductId', Product, self);
				} 
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.
				if (!data.assignedTo) { 
					if (data.assignedToId != self.assignedToId()) {
                        self.assignedTo(null);
                    }
                }else if (!self.assignedTo()){
					self.assignedTo(new Person(data.assignedTo, self));
				}else{
					self.assignedTo().loadFromDto(data.assignedTo);
				}
				if (!data.reportedBy) { 
					if (data.reportedById != self.reportedById()) {
                        self.reportedBy(null);
                    }
                }else if (!self.reportedBy()){
					self.reportedBy(new Person(data.reportedBy, self));
				}else{
					self.reportedBy().loadFromDto(data.reportedBy);
				}
				if (!data.devTeamAssigned) { 
					if (data.devTeamAssignedId != self.devTeamAssignedId()) {
                        self.devTeamAssigned(null);
                    }
                }else if (!self.devTeamAssigned()){
					self.devTeamAssigned(new DevTeam(data.devTeamAssigned, self));
				}else{
					self.devTeamAssigned().loadFromDto(data.devTeamAssigned);
				}

				// The rest of the objects are loaded now.
				self.caseKey(data.caseKey);
				self.title(data.title);
				self.description(data.description);
                if (data.openedAt == null) self.openedAt(null);
				else if (self.openedAt() == null || !self.openedAt().isSame(moment(data.openedAt))){
				    self.openedAt(moment(data.openedAt));
				}
				self.assignedToId(data.assignedToId);
				self.reportedById(data.reportedById);
				self.attachment(data.attachment);
				self.severity(data.severity);
				self.status(data.status);
				self.devTeamAssignedId(data.devTeamAssignedId);
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

			self.save = function(callback?: any) {
				if (!self.isLoading()) {
					if (self.validate()) {
						if (self.showBusyWhenSaving) intellitect.utilities.showBusy();
						self.isSaving(true);
                        $.ajax({ method: "POST", url: areaUrl + "api/Case/Save?includes=" + self.includes, data: self.saveToDto(), xhrFields: { withCredentials: true } })
						.done(function(data) {
							self.isDirty(false);
							if (data.wasSuccessful) {
								self.errorMessage('');
                                if (self.isDataFromSaveLoadedComputed()) {
								    self.loadFromDto(data.object);
                                }
								// The object is now saved. Call any callback.
								for (var i in self.saveCallbacks) {
									self.saveCallbacks[i](self);
								}
							} else {
								self.errorMessage(data.message);
                                self.validationIssues(data.validationIssues);
							}
						})
						.fail(function() {
							alert("Could not save the item.");
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
                    id = self.caseKey();
                }
                if (id) {
                    self.isLoading(true);
                    intellitect.utilities.showBusy();
                    $.ajax({ method: "GET", url: areaUrl + "api/Case/Get/" + id + '?includes=' + self.includes, xhrFields: { withCredentials: true } })
                        .done(function(data) {
                            self.loadFromDto(data);
                            if ($.isFunction(callback)) callback(self);
                        })
                        .fail(function() {
                            alert("Could not get Case with id = " + id);
                        })
                        .always(function() {
                            intellitect.utilities.hideBusy();
                            self.isLoading(false);
                        });
                }
            };

            self.reload = function(callback) {
                self.load(self.caseKey(), callback);
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
                var currentId = self.caseKey();
                if (currentId){
                $.ajax({ method: "POST", url: areaUrl+ "api/Case/Delete/" + currentId, xhrFields: { withCredentials: true } })
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


            // Save a many-to-many collection
            self.saveCollection = function(propertyName, childId, operation) {
                var method = (operation === "added" ? "AddToCollection" : "RemoveFromCollection");
                var currentId = self.caseKey();
                $.ajax({ method: "POST", url: areaUrl + 'api/Case/' + method + '?id=' + currentId + '&propertyName=' + propertyName + '&childId=' + childId, xhrFields: { withCredentials: true } })
                .done(function(data) {
                    if (data.wasSuccessful) {
                        self.errorMessage('');
                        self.loadFromDto(data.object);
                        // The object is now saved. Call any callback.
                        for (var i in self.saveCallbacks) {
                            self.saveCallbacks[i](self);
                        }
                    } else {
                        self.errorMessage(data.message);
                        self.validationIssues(data.validationIssues);
                    }
                })
                .fail(function() {
                    alert("Could not save the item.");
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
                $.ajax({ method: "GET", url: areaUrl + "api/Person/List?Fields=PersonId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.assignedToValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function() {
                    alert("Could not get Valid Values for AssignedTo");
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
                $.ajax({ method: "GET", url: areaUrl + "api/Person/List?Fields=PersonId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.reportedByValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function() {
                    alert("Could not get Valid Values for ReportedBy");
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
                $.ajax({ method: "GET", url: areaUrl + "api/CaseProduct/List?Fields=CaseProductId,CaseProductId", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.caseProductsValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function() {
                    alert("Could not get Valid Values for CaseProducts");
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
                $.ajax({ method: "GET", url: areaUrl + "api/DevTeam/List?Fields=DevTeamId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.devTeamAssignedValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function() {
                    alert("Could not get Valid Values for DevTeamAssigned");
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
                $.ajax({ method: "GET", url: areaUrl + 'Case/EditorHtml', data: {simple: true}, xhrFields: { withCredentials: true } })
                .done(function(data){
                    // Add to DOM
                    intellitect.webApi.setupModal('Edit Case', data, true, false);
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

            self.showAssignedToEditor = function() {
                if (!self.assignedTo()) {
                    self.assignedTo(new Person());
                }
                self.assignedTo().showEditor()
            };
            self.showReportedByEditor = function() {
                if (!self.reportedBy()) {
                    self.reportedBy(new Person());
                }
                self.reportedBy().showEditor()
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
                else self.loadFromDto(newItem);
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
/// <reference path="../../typings/tsd.d.ts" />

var baseUrl = baseUrl || "/";
var saveTimeoutInMs = saveTimeoutInMs || 500;
var saveImmediately: boolean;

class EnumValue {
    id: number;
    value: string;
}

module ViewModels {
    export class BaseViewModel<T extends BaseViewModel<any>> {

        protected modelName: string;
        protected modelDisplayName: string;
        protected areaUrl: string;
        protected primaryKeyName: string;
        protected apiUrlBase: string;
        protected viewUrlBase: string;
        public dataSources: any;

        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public dataSource: any;

        protected loadingCount: number = 0;  // Stack for number of times loading has been called.
        protected saveTimeout: number = 0;   // Stores the return value of setInterval for automatic save delays.
        // Callbacks to call after a delete.
        public deleteCallbacks: { (self: T): void; }[] = [];
        // Callbacks to call after a save.
        public saveCallbacks: { (self: T): void; }[] = [];
        protected loadingValidValues: number = 0;

        // String that defines what data should be included with the returned object.
        public includes = null;


        // If true, the busy indicator is shown when loading.
        public showBusyWhenSaving = false;  // If true a busy indicator shows when saving.
        // Whether or not alerts should be shown when loading fails.
        public showFailureAlerts: boolean = true;

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
        public changeIsExpanded = (value?: boolean) => { // Call this with the edit button.
            if (value !== true || value !== false) this.isExpanded(!this.isExpanded());
            else this.isExpanded(value === true); // Force boolean
        };
        // Flag to use to determine if this item is being edited. Only for convenience.
        public isEditing = ko.observable(false);
        // Alternates the isEditing flag. Use with a click binding for a button.
        public changeIsEditing = (value) => {  // Call this with the edit button.
            if (value !== true || value !== false) this.isEditing(!this.isEditing());
            else this.isEditing(value === true);  // Force boolean
        };
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

        // Set this false when editing a field that saves periodically while the user is typing. 
        // By default(null), isDataFromSaveLoadedComputed will check the parent's value. 
        // If the topmost parent is null, the value is true. Otherwise the first set value will be returned.
        public isDataFromSaveLoaded: boolean = null;
        public isValid: () => boolean;


        // Assign this function to add validation that prevents saving  by returning false.
        // Return true to continue to save.
        public validate = () => {
            this.errors.showAllMessages();
            this.warnings.showAllMessages();
            return this.errors().length == 0;
        };


        // Deletes the object after a confirmation box.
        public deleteItemWithConfirmation = (callback?: () => void, message?: string) => {
            if (typeof message != 'string') {
                message = "Delete this item?";
            }
            if (confirm(message)) {
                this.deleteItem(callback);
            }
        };

        // True if the object is loading.
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        // True once the data has been loaded.
        public isLoaded: KnockoutObservable<boolean> = ko.observable(false);
        // URL to a stock editor for this object.
        public editUrl: () => string;


        // Loads this object from a data transfer object received from the server.
        // Force: Will override the check against isLoading that is done to prevent recursion.
        // AllowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
        public loadFromDto: (data: any, force?: boolean, allowCollectionDeletes?: boolean) => void;
        // Called at the end of loadFromDto to allow for custom code like sorting child collections.
        public afterLoadFromDto: () => void;
        // Saves this object into a data transfer object to send to the server.
        public saveToDto: () => any;

        // Loads collections of valid values. May be removed.
        public loadValidValues: (callback?: () => void) => void;
        // Loads any children that have an ID but have not been loaded. 
        // This is useful when creating an object that has a parent object and the ID is set on the new child.
        public loadChildren: (callback?: () => void) => void;




        // Code to handle saving flags.
        // Returns true if this object or any of its children is saving.
        public isSavingWithChildren = ko.computed(() => {
            if (this.isSaving()) return true;
            if (this.savingChildCount() > 0) return true;
            return false;
        });

        // Handle children that are saving.
        // Internally used member to count the number of saving children.
        public savingChildChange = (isSaving: boolean) => {
            if (isSaving) this.savingChildCount(this.savingChildCount() + 1);
            else this.savingChildCount(this.savingChildCount() - 1);
            if (this.parent && $.isFunction(this.parent.savingChildChange)) {
                this.parent.savingChildChange(isSaving);
            }
        };

        // Code to handle isDataFromSaveLoaded
        // Used internally to determine if the data from a save operation should be loaded.
        public isDataFromSaveLoadedComputed = () => {
            if (this.isDataFromSaveLoaded === false) return false;
            if (this.isDataFromSaveLoaded === true) return true;
            if (this.parent && $.isFunction(this.parent.isDataFromSaveLoadedComputed)) {
                return this.parent.isDataFromSaveLoadedComputed();
            }
            return true;
        };


        // Saves the object to the server and then calls the callback.
        public save = (callback?: (self: T) => void) => {
            if (!this.isLoading()) {
                if (this.validate()) {
                    if (this.showBusyWhenSaving) intellitect.utilities.showBusy();
                    this.isSaving(true);


                    var url = this.areaUrl + this.apiUrlBase + "/Save?includes=" + this.includes + '&dataSource=';
                    if (typeof this.dataSource === "string") url += this.dataSource;
                    else url += this.dataSources[this.dataSource];

                    $.ajax({ method: "POST", url: url, data: this.saveToDto(), xhrFields: { withCredentials: true } })
                        .done((data) => {
                            this.isDirty(false);
                            this.errorMessage('');
                            if (this.isDataFromSaveLoadedComputed()) {
                                this.loadFromDto(data.object, true);
                            }
                            // The object is now saved. Call any callback.
                            for (var i in this.saveCallbacks) {
                                this.saveCallbacks[i](this as any as T);
                            }
                        })
                        .fail((xhr) => {
                            var errorMsg = "Unknown Error";
                            var validationIssues = [];
                            if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                            if (xhr.responseJSON && xhr.responseJSON.validationIssues) validationIssues = xhr.responseJSON.validationIssues;
                            this.errorMessage(errorMsg);
                            this.validationIssues(validationIssues);
                            // If an object was returned, load that object.
                            if (xhr.responseJSON && xhr.responseJSON.object) {
                                this.loadFromDto(xhr.responseJSON.object, true);
                            }
                            if (this.showFailureAlerts)
                                alert("Could not save the item: " + errorMsg);
                        })
                        .always(() => {
                            this.isSaving(false);
                            if ($.isFunction(callback)) {
                                callback(this as any as T);
                            }
                            if (this.showBusyWhenSaving) intellitect.utilities.hideBusy();
                        });
                }
                else {
                    // If validation fails, we still want to try and load any child objects which may have just been set.
                    // Normally, we get these from the result of the save.
                    this.loadChildren();
                }
            }
        }


        // Loads the object from the server based on the id specified. Once complete calls the callback.
        public load = (id: any, callback?: (self: T) => void) => {
            if (!id) {
                id = this[this.primaryKeyName]();
            }
            if (id) {
                this.isLoading(true);
                intellitect.utilities.showBusy();

                var url = this.areaUrl + this.apiUrlBase + "/Get/" + id + '?includes=' + this.includes + '&dataSource=';
                if (typeof this.dataSource === "string") url += this.dataSource;
                else url += this.dataSources[this.dataSource];

                $.ajax({ method: "GET", url: url, xhrFields: { withCredentials: true } })
                    .done((data) => {
                        this.loadFromDto(data, true);
                        this.isLoaded(true);
                        if ($.isFunction(callback)) callback(this as any as T);
                    })
                    .fail(() => {
                        this.isLoaded(false);
                        if (this.showFailureAlerts)
                            alert("Could not get " + this.modelName + " with id = " + id);
                    })
                    .always(() => {
                        intellitect.utilities.hideBusy();
                        this.isLoading(false);
                    });
            }
        };

        // Reloads the object from the server.
        public reload = (callback?: () => void) => {
            this.load(this[this.primaryKeyName](), callback);
        };

        // Deletes the object without confirmation.
        public deleteItem = (callback?: (self: T) => void) => {
            var currentId = this[this.primaryKeyName]();
            if (currentId) {
                $.ajax({ method: "POST", url: this.areaUrl + this.apiUrlBase + "/Delete/" + currentId, xhrFields: { withCredentials: true } })
                    .done((data) => {
                        if (data) {
                            this.errorMessage('');
                            // The object is now deleted. Call any callback.
                            for (var i in this.deleteCallbacks) {
                                this.deleteCallbacks[i](this as any as T);
                            }
                            // Remove it from the parent collection
                            if (this.parentCollection && this.parent) {
                                this.parent.isLoading(true);
                                this.parentCollection.splice(this.parentCollection().indexOf(this), 1);
                                this.parent.isLoading(false);
                            }
                        } else {
                            this.errorMessage(data.message);
                        }
                    })
                    .fail(() => {
                        if (this.showFailureAlerts)
                            alert("Could not delete the item.");
                    })
                    .always(() => {
                        if ($.isFunction(callback)) {
                            callback(this as any as T);
                        }
                    });
            } else {
                // No ID has been assigned yet, just remove it.
                if (this.parentCollection && this.parent) {
                    this.parent.isLoading(true);
                    this.parentCollection.splice(this.parentCollection().indexOf(this), 1);
                    this.parent.isLoading(false);
                }
                if ($.isFunction(callback)) {
                    callback(this as any as T);
                }
            }
        };

        // Sets isSelected(true) on this object and clears on the rest of the items in the parentCollection.
        // Returns true to bubble additional click events.
        public selectSingle = () => {
            if (this.parentCollection()) {
                $.each(this.parentCollection(), (i, obj) => {
                    obj.isSelected(false);
                });
            }
            this.isSelected(true);
            return true; // Allow other click events
        };

        // Toggles isSelected value. Returns true to bubble additional click events.
        public isSelectedToggle = () => {
            this.isSelected(!this.isSelected());
            return true;
        }


        // Saves a many-to-many collection change. This is done automatically and doesn't need to be called.
        public saveCollection = (propertyName: string, childId: any, operation: string) => {
            var method = (operation === "added" ? "AddToCollection" : "RemoveFromCollection");
            var currentId = this[this.primaryKeyName]();
            $.ajax({ method: "POST", url: this.areaUrl + this.apiUrlBase + '/' + method + '?id=' + currentId + '&propertyName=' + propertyName + '&childId=' + childId, xhrFields: { withCredentials: true } })
                .done((data) => {
                    this.errorMessage('');
                    this.loadFromDto(data.object, true);
                    // The object is now saved. Call any callback.
                    for (var i in this.saveCallbacks) {
                        this.saveCallbacks[i](this as any as T);
                    }
                })
                .fail((xhr) => {
                    var errorMsg = "Unknown Error";
                    var validationIssues = [];
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    if (xhr.responseJSON && xhr.responseJSON.validationIssues) errorMsg = xhr.responseJSON.validationIssues;
                    this.errorMessage(errorMsg);
                    this.validationIssues(validationIssues);

                    if (this.showFailureAlerts)
                        alert("Could not save the item: " + errorMsg);
                })
                .always(() => {
                    // Nothing here yet.
                });
        };


        // Callback to be called when this item is deleted.
        public onDelete = (fn) => {
            if ($.isFunction(fn)) this.deleteCallbacks.push(fn);
        };

        // Callback to be called when a save is done.
        public onSave = (fn) => {
            if ($.isFunction(fn)) this.saveCallbacks.push(fn);
        };

        // Saves the object is isSavingAutomatically is true.
        public autoSave = () => {
            if (!this.isLoading()) {
                this.isDirty(true);
                if (this.isSavingAutomatically) {
                    // Batch saves.
                    if (!this.saveTimeout) {
                        this.saveTimeout = setTimeout(() => {
                            this.saveTimeout = 0;
                            // If we have a save in progress, wait...
                            if (this.isSaving()) {
                                this.autoSave();
                            } else {
                                this.save();
                            }
                        }, saveTimeoutInMs);
                    }
                }
            }
        }

        // Saves many to many collections if isSavingAutomatically is true.
        public autoSaveCollection = (property: string, id: any, changeStatus: string) => {
            if (!this.isLoading()) {
                // TODO: Eventually Batch saves for many-to-many collections.
                if (changeStatus === 'added') {
                    this.saveCollection(property, id, "added");
                } else if (changeStatus === 'deleted') {
                    this.saveCollection(property, id, "deleted");
                }
            }
        }

        // Supply methods to pop up a model editor
        public showEditor = () => {
            // Close any existing modal
            $('#modal-dialog').modal('hide');
            // Get new modal content
            intellitect.utilities.showBusy();
            $.ajax({ method: "GET", url: this.areaUrl + this.viewUrlBase + '/EditorHtml', data: { simple: true }, xhrFields: { withCredentials: true } })
                .done((data) => {
                    // Add to DOM
                    intellitect.webApi.setupModal('Edit ' + this.modelDisplayName, data, true, false);
                    // Data bind
                    var lastValue = this.isSavingAutomatically;
                    this.isSavingAutomatically = false;
                    ko.applyBindings(self, document.getElementById("modal-dialog"));
                    this.isSavingAutomatically = lastValue;
                    // Show the dialog
                    $('#modal-dialog').modal('show');
                })
                .always(() => {
                    intellitect.utilities.hideBusy();
                });
        }

        constructor() {
            // Handles setting the parent savingChildChange
            this.isSaving.subscribe((newValue: boolean) => {
                if (this.parent && $.isFunction(this.parent.savingChildChange)) {
                    this.parent.savingChildChange(newValue);
                }
            })
        }
    }
}

module ListViewModels {
    export class BaseListViewModel<T, TItem extends ViewModels.BaseViewModel<any>> {

        protected modelName: string;
        protected areaUrl: string;
        protected apiUrlBase: string;
        public dataSources: any;

        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public listDataSource: any;

        // Query string to limit the list of items.
        public queryString: string = "";
        // Object that is passed as the query parameters.
        public query: any = null;

        // String the represents the child object to load 
        public includes: string = "";
        // Whether or not alerts should be shown when loading fails.
        public showFailureAlerts: boolean = true;
        // List of items. This the main collection.
        public items: KnockoutObservableArray<TItem> = ko.observableArray([]);
        // Load the list.
        public load = (callback?: any) => {
                intellitect.utilities.showBusy();
                if (this.query) {
                    this.queryString = $.param(this.query);
                }
                this.isLoading(true);

                var url = this.areaUrl + this.apiUrlBase + "/List?includes=" + this.includes + "&page=" + this.page()
                    + "&pageSize=" + this.pageSize() + "&search=" + this.search()
                    + "&orderBy=" + this.orderBy() + "&orderByDescending=" + this.orderByDescending()
                            + "&listDataSource=";
    
                if (typeof this.listDataSource === "string") url += this.listDataSource;
                else url += this.dataSources[this.listDataSource];

                if (this.queryString !== null && this.queryString !== "") url += "&" + this.queryString;

                $.ajax({ method: "GET",
                         url: url,
                        xhrFields: { withCredentials: true } })
                .done((data) => {
                    this.items.removeAll();
                    for (var i in data.list) {
                        var model = this.createItem(data.list[i]);
                        model.includes = this.includes;
                        model.onDelete((item) => {
                            this.items.remove(item);
                        });
                        this.items.push(model);
                    }
                    this.count(data.list.length);
                    this.totalCount(data.totalCount);
                    this.pageCount(data.pageCount);
                    this.page(data.page);
                    this.message(data.message)
                    this.isLoaded(true);
                    if ($.isFunction(callback)) callback(this);
                })
                .fail((xhr) => {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    this.message(errorMsg);
                    this.isLoaded(false);
                    
                    if (this.showFailureAlerts)
                        alert("Could not get list of " + this.modelName + " items: " + errorMsg);
                })
                .always(() => {
                    intellitect.utilities.hideBusy();
                    this.isLoading(false);
                });
        };
        protected createItem: (newItem?: any, parent?: any) => TItem;
        // Adds a new item to the collection.
        public addNewItem = () => {
            var item = this.createItem();
            this.items.push(item);
            return item;
        };;
        // Deletes an item.
        public deleteItem = (item: TItem) => {
            item.deleteItem();
        };;
        // True if the collection is loading.
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Gets the count of items without getting all the items. Data put into count.
        public getCount = (callback?: any) => {
            intellitect.utilities.showBusy();
            if (this.query) {
                this.queryString = $.param(this.query);
            }
            $.ajax({
                method: "GET",
                url: this.areaUrl + this.apiUrlBase + "/count?" + "listDataSource="
                    + this.dataSources[this.listDataSource] + "&" + this.queryString,
                        xhrFields: { withCredentials: true } })
            .done((data) => {
                this.count(data);
                if ($.isFunction(callback)) callback();
            })
            .fail(() => {
                if (this.showFailureAlerts)
                    alert("Could not get count of " + this.modelName + " items.");
            })
            .always(() => {
                intellitect.utilities.hideBusy();
            });
        };
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
        public nextPageEnabled = ko.computed(() => this.page() < this.pageCount());

        // If there is a previous page, this is true.
        public previousPageEnabled = ko.computed(() => this.page() > 1);

        // Gets the next page.
        public nextPage = () => {
            if (this.nextPageEnabled()) {
                this.page(this.page() + 1);
            }
        };
        // Gets the previous page.
        public previousPage = () => {
            if (this.previousPageEnabled()) {
                this.page(this.page() - 1);
            }
        };

        // Control order of results
        public orderBy: KnockoutObservable<string> = ko.observable("");
        public orderByDescending: KnockoutObservable<string> = ko.observable("");

        // True once the data has been loaded.
        public isLoaded: KnockoutObservable<boolean> = ko.observable(false);

        public constructor() {
            var searchTimeout: number = 0;

            this.pageSize.subscribe(() => {
                if (this.isLoaded()) {
                    this.load();
                }
            });
            this.page.subscribe(() => {
                if (this.isLoaded() && !this.isLoading()) {
                    this.load();
                }
            });
            this.search.subscribe(() => {
                if (searchTimeout) {
                    clearTimeout(searchTimeout);
                }
                searchTimeout = setTimeout(() => {
                    searchTimeout = 0;
                    this.load();
                }, 300);
            });
        }
    }
}
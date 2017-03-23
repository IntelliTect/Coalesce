/// <reference path="../../typings/tsd.d.ts" />
var baseUrl = baseUrl || "/";
var saveTimeoutInMs = saveTimeoutInMs || 500;
var saveImmediately;
var EnumValue = (function () {
    function EnumValue() {
    }
    return EnumValue;
}());
var ViewModels;
(function (ViewModels) {
    var BaseViewModel = (function () {
        function BaseViewModel() {
            var _this = this;
            this.loadingCount = 0; // Stack for number of times loading has been called.
            this.saveTimeout = 0; // Stores the return value of setInterval for automatic save delays.
            // Callbacks to call after a delete.
            this.deleteCallbacks = [];
            // Callbacks to call after a save.
            this.saveCallbacks = [];
            this.loadingValidValues = 0;
            // String that defines what data should be included with the returned object.
            this.includes = null;
            // If true, the busy indicator is shown when loading.
            this.showBusyWhenSaving = false; // If true a busy indicator shows when saving.
            // Whether or not alerts should be shown when loading fails.
            this.showFailureAlerts = true;
            // Parent of this object.
            this.parent = null;
            // Collection that this object is a part of.
            this.parentCollection = null;
            // ID of the object.
            this.myId = 0;
            // Dirty Flag
            this.isDirty = ko.observable(false);
            // Error message for the page
            this.errorMessage = ko.observable(null);
            // ValidationIssues returned from database when trying to persist data
            this.validationIssues = ko.observableArray([]);
            // If this is true, all changes will be saved automatically.
            this.isSavingAutomatically = true;
            // Flag to use to determine if this item is shown. Only for convenience.
            this.isVisible = ko.observable(false);
            // Flag to use to determine if this item is expanded. Only for convenience.
            this.isExpanded = ko.observable(false);
            // Flag to use to determine if this item is selected. Only for convenience.
            this.isSelected = ko.observable(false);
            // Flag to use to determine if this item is checked. Only for convenience.
            this.isChecked = ko.observable(false);
            // Alternates the isExpanded flag. Use with a click binding for a button.
            this.changeIsExpanded = function (value) {
                if (value !== true || value !== false)
                    _this.isExpanded(!_this.isExpanded());
                else
                    _this.isExpanded(value === true); // Force boolean
            };
            // Flag to use to determine if this item is being edited. Only for convenience.
            this.isEditing = ko.observable(false);
            // Alternates the isEditing flag. Use with a click binding for a button.
            this.changeIsEditing = function (value) {
                if (value !== true || value !== false)
                    _this.isEditing(!_this.isEditing());
                else
                    _this.isEditing(value === true); // Force boolean
            };
            // List of errors found during validation.
            this.errors = ko.observableArray([]);
            // List of warnings found during validation. These allow a save.
            this.warnings = ko.observableArray([]);
            // Custom Field that can be used via scripts. This allows for setting observables via scripts and using them without modifying the ViewModel
            this.customField1 = ko.observable();
            // Custom Field 2 that can be used via scripts. This allows for setting observables via scripts and using them without modifying the ViewModel
            this.customField2 = ko.observable();
            // Custom Field 3 that can be used via scripts. This allows for setting observables via scripts and using them without modifying the ViewModel
            this.customField3 = ko.observable();
            // True if the object is currently saving.
            this.isSaving = ko.observable(false);
            // Internal count of child objects that are saving.
            this.savingChildCount = ko.observable(0);
            // Set this false when editing a field that saves periodically while the user is typing. 
            // By default(null), isDataFromSaveLoadedComputed will check the parent's value. 
            // If the topmost parent is null, the value is true. Otherwise the first set value will be returned.
            this.isDataFromSaveLoaded = null;
            // Assign this function to add validation that prevents saving  by returning false.
            // Return true to continue to save.
            this.validate = function () {
                _this.errors.showAllMessages();
                _this.warnings.showAllMessages();
                return _this.errors().length == 0;
            };
            // Deletes the object after a confirmation box.
            this.deleteItemWithConfirmation = function (callback, message) {
                if (typeof message != 'string') {
                    message = "Delete this item?";
                }
                if (confirm(message)) {
                    _this.deleteItem(callback);
                }
            };
            // True if the object is loading.
            this.isLoading = ko.observable(false);
            // True once the data has been loaded.
            this.isLoaded = ko.observable(false);
            // Code to handle saving flags.
            // Returns true if this object or any of its children is saving.
            this.isSavingWithChildren = ko.computed(function () {
                if (_this.isSaving())
                    return true;
                if (_this.savingChildCount() > 0)
                    return true;
                return false;
            });
            // Handle children that are saving.
            // Internally used member to count the number of saving children.
            this.savingChildChange = function (isSaving) {
                if (isSaving)
                    _this.savingChildCount(_this.savingChildCount() + 1);
                else
                    _this.savingChildCount(_this.savingChildCount() - 1);
                if (_this.parent && $.isFunction(_this.parent.savingChildChange)) {
                    _this.parent.savingChildChange(isSaving);
                }
            };
            // Code to handle isDataFromSaveLoaded
            // Used internally to determine if the data from a save operation should be loaded.
            this.isDataFromSaveLoadedComputed = function () {
                if (_this.isDataFromSaveLoaded === false)
                    return false;
                if (_this.isDataFromSaveLoaded === true)
                    return true;
                if (_this.parent && $.isFunction(_this.parent.isDataFromSaveLoadedComputed)) {
                    return _this.parent.isDataFromSaveLoadedComputed();
                }
                return true;
            };
            // Saves the object to the server and then calls the callback.
            this.save = function (callback) {
                if (!_this.isLoading()) {
                    if (_this.validate()) {
                        if (_this.showBusyWhenSaving)
                            intellitect.utilities.showBusy();
                        _this.isSaving(true);
                        var url = _this.areaUrl + _this.apiUrlBase + "/Save?includes=" + _this.includes + '&dataSource=';
                        if (typeof _this.dataSource === "string")
                            url += _this.dataSource;
                        else
                            url += _this.dataSources[_this.dataSource];
                        $.ajax({ method: "POST", url: url, data: _this.saveToDto(), xhrFields: { withCredentials: true } })
                            .done(function (data) {
                            _this.isDirty(false);
                            _this.errorMessage('');
                            if (_this.isDataFromSaveLoadedComputed()) {
                                _this.loadFromDto(data.object, true);
                            }
                            // The object is now saved. Call any callback.
                            for (var i in _this.saveCallbacks) {
                                _this.saveCallbacks[i](_this);
                            }
                        })
                            .fail(function (xhr) {
                            var errorMsg = "Unknown Error";
                            var validationIssues = [];
                            if (xhr.responseJSON && xhr.responseJSON.message)
                                errorMsg = xhr.responseJSON.message;
                            if (xhr.responseJSON && xhr.responseJSON.validationIssues)
                                validationIssues = xhr.responseJSON.validationIssues;
                            _this.errorMessage(errorMsg);
                            _this.validationIssues(validationIssues);
                            // If an object was returned, load that object.
                            if (xhr.responseJSON && xhr.responseJSON.object) {
                                _this.loadFromDto(xhr.responseJSON.object, true);
                            }
                            if (_this.showFailureAlerts)
                                alert("Could not save the item: " + errorMsg);
                        })
                            .always(function () {
                            _this.isSaving(false);
                            if ($.isFunction(callback)) {
                                callback(_this);
                            }
                            if (_this.showBusyWhenSaving)
                                intellitect.utilities.hideBusy();
                        });
                    }
                    else {
                        // If validation fails, we still want to try and load any child objects which may have just been set.
                        // Normally, we get these from the result of the save.
                        _this.loadChildren();
                    }
                }
            };
            // Loads the object from the server based on the id specified. Once complete calls the callback.
            this.load = function (id, callback) {
                if (!id) {
                    id = _this[_this.primaryKeyName]();
                }
                if (id) {
                    _this.isLoading(true);
                    intellitect.utilities.showBusy();
                    var url = _this.areaUrl + _this.apiUrlBase + "/Get/" + id + '?includes=' + _this.includes + '&dataSource=';
                    if (typeof _this.dataSource === "string")
                        url += _this.dataSource;
                    else
                        url += _this.dataSources[_this.dataSource];
                    $.ajax({ method: "GET", url: url, xhrFields: { withCredentials: true } })
                        .done(function (data) {
                        _this.loadFromDto(data, true);
                        _this.isLoaded(true);
                        if ($.isFunction(callback))
                            callback(_this);
                    })
                        .fail(function () {
                        _this.isLoaded(false);
                        if (_this.showFailureAlerts)
                            alert("Could not get " + _this.modelName + " with id = " + id);
                    })
                        .always(function () {
                        intellitect.utilities.hideBusy();
                        _this.isLoading(false);
                    });
                }
            };
            // Reloads the object from the server.
            this.reload = function (callback) {
                _this.load(_this[_this.primaryKeyName](), callback);
            };
            // Deletes the object without confirmation.
            this.deleteItem = function (callback) {
                var currentId = _this[_this.primaryKeyName]();
                if (currentId) {
                    $.ajax({ method: "POST", url: _this.areaUrl + _this.apiUrlBase + "/Delete/" + currentId, xhrFields: { withCredentials: true } })
                        .done(function (data) {
                        if (data) {
                            _this.errorMessage('');
                            // The object is now deleted. Call any callback.
                            for (var i in _this.deleteCallbacks) {
                                _this.deleteCallbacks[i](_this);
                            }
                            // Remove it from the parent collection
                            if (_this.parentCollection && _this.parent) {
                                _this.parent.isLoading(true);
                                _this.parentCollection.splice(_this.parentCollection().indexOf(_this), 1);
                                _this.parent.isLoading(false);
                            }
                        }
                        else {
                            _this.errorMessage(data.message);
                        }
                    })
                        .fail(function () {
                        if (_this.showFailureAlerts)
                            alert("Could not delete the item.");
                    })
                        .always(function () {
                        if ($.isFunction(callback)) {
                            callback(_this);
                        }
                    });
                }
                else {
                    // No ID has been assigned yet, just remove it.
                    if (_this.parentCollection && _this.parent) {
                        _this.parent.isLoading(true);
                        _this.parentCollection.splice(_this.parentCollection().indexOf(_this), 1);
                        _this.parent.isLoading(false);
                    }
                    if ($.isFunction(callback)) {
                        callback(_this);
                    }
                }
            };
            // Sets isSelected(true) on this object and clears on the rest of the items in the parentCollection.
            // Returns true to bubble additional click events.
            this.selectSingle = function () {
                if (_this.parentCollection()) {
                    $.each(_this.parentCollection(), function (i, obj) {
                        obj.isSelected(false);
                    });
                }
                _this.isSelected(true);
                return true; // Allow other click events
            };
            // Toggles isSelected value. Returns true to bubble additional click events.
            this.isSelectedToggle = function () {
                _this.isSelected(!_this.isSelected());
                return true;
            };
            // Saves a many-to-many collection change. This is done automatically and doesn't need to be called.
            this.saveCollection = function (propertyName, childId, operation) {
                var method = (operation === "added" ? "AddToCollection" : "RemoveFromCollection");
                var currentId = _this[_this.primaryKeyName]();
                $.ajax({ method: "POST", url: _this.areaUrl + _this.apiUrlBase + '/' + method + '?id=' + currentId + '&propertyName=' + propertyName + '&childId=' + childId, xhrFields: { withCredentials: true } })
                    .done(function (data) {
                    _this.errorMessage('');
                    _this.loadFromDto(data.object, true);
                    // The object is now saved. Call any callback.
                    for (var i in _this.saveCallbacks) {
                        _this.saveCallbacks[i](_this);
                    }
                })
                    .fail(function (xhr) {
                    var errorMsg = "Unknown Error";
                    var validationIssues = [];
                    if (xhr.responseJSON && xhr.responseJSON.message)
                        errorMsg = xhr.responseJSON.message;
                    if (xhr.responseJSON && xhr.responseJSON.validationIssues)
                        errorMsg = xhr.responseJSON.validationIssues;
                    _this.errorMessage(errorMsg);
                    _this.validationIssues(validationIssues);
                    if (_this.showFailureAlerts)
                        alert("Could not save the item: " + errorMsg);
                })
                    .always(function () {
                    // Nothing here yet.
                });
            };
            // Callback to be called when this item is deleted.
            this.onDelete = function (fn) {
                if ($.isFunction(fn))
                    _this.deleteCallbacks.push(fn);
            };
            // Callback to be called when a save is done.
            this.onSave = function (fn) {
                if ($.isFunction(fn))
                    _this.saveCallbacks.push(fn);
            };
            // Saves the object is isSavingAutomatically is true.
            this.autoSave = function () {
                if (!_this.isLoading()) {
                    _this.isDirty(true);
                    if (_this.isSavingAutomatically) {
                        // Batch saves.
                        if (!_this.saveTimeout) {
                            _this.saveTimeout = setTimeout(function () {
                                _this.saveTimeout = 0;
                                // If we have a save in progress, wait...
                                if (_this.isSaving()) {
                                    _this.autoSave();
                                }
                                else {
                                    _this.save();
                                }
                            }, saveTimeoutInMs);
                        }
                    }
                }
            };
            // Saves many to many collections if isSavingAutomatically is true.
            this.autoSaveCollection = function (property, id, changeStatus) {
                if (!_this.isLoading()) {
                    // TODO: Eventually Batch saves for many-to-many collections.
                    if (changeStatus === 'added') {
                        _this.saveCollection(property, id, "added");
                    }
                    else if (changeStatus === 'deleted') {
                        _this.saveCollection(property, id, "deleted");
                    }
                }
            };
            // Supply methods to pop up a model editor
            this.showEditor = function () {
                // Close any existing modal
                $('#modal-dialog').modal('hide');
                // Get new modal content
                intellitect.utilities.showBusy();
                $.ajax({ method: "GET", url: _this.areaUrl + _this.viewUrlBase + '/EditorHtml', data: { simple: true }, xhrFields: { withCredentials: true } })
                    .done(function (data) {
                    // Add to DOM
                    intellitect.webApi.setupModal('Edit ' + _this.modelDisplayName, data, true, false);
                    // Data bind
                    var lastValue = _this.isSavingAutomatically;
                    _this.isSavingAutomatically = false;
                    ko.applyBindings(self, document.getElementById("modal-dialog"));
                    _this.isSavingAutomatically = lastValue;
                    // Show the dialog
                    $('#modal-dialog').modal('show');
                })
                    .always(function () {
                    intellitect.utilities.hideBusy();
                });
            };
            // Handles setting the parent savingChildChange
            this.isSaving.subscribe(function (newValue) {
                if (_this.parent && $.isFunction(_this.parent.savingChildChange)) {
                    _this.parent.savingChildChange(newValue);
                }
            });
        }
        return BaseViewModel;
    }());
    ViewModels.BaseViewModel = BaseViewModel;
})(ViewModels || (ViewModels = {}));
var ListViewModels;
(function (ListViewModels) {
    var BaseListViewModel = (function () {
        function BaseListViewModel() {
            var _this = this;
            // Query string to limit the list of items.
            this.queryString = "";
            // Object that is passed as the query parameters.
            this.query = null;
            // String the represents the child object to load 
            this.includes = "";
            // Whether or not alerts should be shown when loading fails.
            this.showFailureAlerts = true;
            // List of items. This the main collection.
            this.items = ko.observableArray([]);
            // Load the list.
            this.load = function (callback) {
                intellitect.utilities.showBusy();
                if (_this.query) {
                    _this.queryString = $.param(_this.query);
                }
                _this.isLoading(true);
                var url = _this.areaUrl + _this.apiUrlBase + "/List?" + _this.queryParams();
                if (typeof _this.listDataSource === "string")
                    url += _this.listDataSource;
                else
                    url += _this.dataSources[_this.listDataSource];
                if (_this.queryString !== null && _this.queryString !== "")
                    url += "&" + _this.queryString;
                $.ajax({ method: "GET",
                    url: url,
                    xhrFields: { withCredentials: true } })
                    .done(function (data) {
                    _this.items.removeAll();
                    for (var i in data.list) {
                        var model = _this.createItem(data.list[i]);
                        model.includes = _this.includes;
                        model.onDelete(function (item) {
                            _this.items.remove(item);
                        });
                        _this.items.push(model);
                    }
                    _this.count(data.list.length);
                    _this.totalCount(data.totalCount);
                    _this.pageCount(data.pageCount);
                    _this.page(data.page);
                    _this.message(data.message);
                    _this.isLoaded(true);
                    if ($.isFunction(callback))
                        callback(_this);
                })
                    .fail(function (xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message)
                        errorMsg = xhr.responseJSON.message;
                    _this.message(errorMsg);
                    _this.isLoaded(false);
                    if (_this.showFailureAlerts)
                        alert("Could not get list of " + _this.modelName + " items: " + errorMsg);
                })
                    .always(function () {
                    intellitect.utilities.hideBusy();
                    _this.isLoading(false);
                });
            };
            this.queryParams = function (pageSize) {
                var query = "includes=" + _this.includes + "&page=" + _this.page()
                    + "&pageSize=" + (pageSize || _this.pageSize()) + "&search=" + _this.search()
                    + "&orderBy=" + _this.orderBy() + "&orderByDescending=" + _this.orderByDescending()
                    + "&listDataSource=";
                return query;
            };
            // Adds a new item to the collection.
            this.addNewItem = function () {
                var item = _this.createItem();
                _this.items.push(item);
                return item;
            };
            // Deletes an item.
            this.deleteItem = function (item) {
                item.deleteItem();
            };
            // True if the collection is loading.
            this.isLoading = ko.observable(false);
            // Gets the count of items without getting all the items. Data put into count.
            this.getCount = function (callback) {
                intellitect.utilities.showBusy();
                if (_this.query) {
                    _this.queryString = $.param(_this.query);
                }
                $.ajax({
                    method: "GET",
                    url: _this.areaUrl + _this.apiUrlBase + "/count?" + "listDataSource="
                        + _this.dataSources[_this.listDataSource] + "&" + _this.queryString,
                    xhrFields: { withCredentials: true } })
                    .done(function (data) {
                    _this.count(data);
                    if ($.isFunction(callback))
                        callback();
                })
                    .fail(function () {
                    if (_this.showFailureAlerts)
                        alert("Could not get count of " + _this.modelName + " items.");
                })
                    .always(function () {
                    intellitect.utilities.hideBusy();
                });
            };
            // The result of getCount() or the total on this page.
            this.count = ko.observable(null);
            // Total count of items, even ones that are not on the page.
            this.totalCount = ko.observable(null);
            // Total page count
            this.pageCount = ko.observable(null);
            // Page number. This can be set to get a new page.
            this.page = ko.observable(1);
            // Number of items on a page.
            this.pageSize = ko.observable(10);
            // If a load failed, this is a message about why it failed.
            this.message = ko.observable(null);
            // Search criteria for the list. This can be exposed as a text box for searching.
            this.search = ko.observable("");
            // Specify the DTO that should be returned - must be a fully qualified type name
            this.dto = ko.observable("");
            // If there is another page, this is true.
            this.nextPageEnabled = ko.computed(function () { return _this.page() < _this.pageCount(); });
            // If there is a previous page, this is true.
            this.previousPageEnabled = ko.computed(function () { return _this.page() > 1; });
            // Gets the next page.
            this.nextPage = function () {
                if (_this.nextPageEnabled()) {
                    _this.page(_this.page() + 1);
                }
            };
            // Gets the previous page.
            this.previousPage = function () {
                if (_this.previousPageEnabled()) {
                    _this.page(_this.page() - 1);
                }
            };
            // Control order of results
            // Set to field name to order by ascending.
            this.orderBy = ko.observable("");
            // Set to field name to order by descending.
            this.orderByDescending = ko.observable("");
            // Set to field name to toggle ordering, ascending, descending, none.
            this.orderByToggle = function (field) {
                if (_this.orderBy() == field && !_this.orderByDescending()) {
                    _this.orderBy('');
                    _this.orderByDescending(field);
                }
                else if (!_this.orderBy() && _this.orderByDescending() == field) {
                    _this.orderBy('');
                    _this.orderByDescending('');
                }
                else {
                    _this.orderBy(field);
                    _this.orderByDescending('');
                }
            };
            // True once the data has been loaded.
            this.isLoaded = ko.observable(false);
            // Gets a URL to download a CSV for the current list with all elements.
            this.downloadAllCsvUrl = ko.computed(function () {
                var url = _this.areaUrl + _this.apiUrlBase + "/CsvDownload?" + _this.queryParams(10000);
                return url;
            });
            // Starts an upload of a CSV file
            this.csvUploadUi = function (callback) {
                // Remove the form if it exists.
                $('#csv-upload').remove();
                // Add the form to the page to take the input
                $('body')
                    .append('<form id="csv-upload" display="none"></form>');
                $('#csv-upload')
                    .attr("action", _this.areaUrl + _this.apiUrlBase + "/CsvUpload").attr("method", "post")
                    .append('<input type="file" style="visibility: hidden;" name="file"/>');
                var self = _this; // The next call messes up 'this' for TypeScript...
                // Set up the click callback.
                $('#csv-upload input[type=file]').change(function () {
                    // Get the files
                    var fileInput = $('#csv-upload input[type=file]')[0];
                    var file = fileInput.files[0];
                    if (file) {
                        var formData = new FormData();
                        formData.append('file', file);
                        intellitect.utilities.showBusy();
                        self.isLoading(true);
                        $.ajax({
                            url: self.areaUrl + self.apiUrlBase + "/CsvUpload",
                            data: formData,
                            processData: false,
                            contentType: false,
                            type: 'POST'
                        })
                            .done(function (data) {
                            self.isLoading(false);
                            if ($.isFunction(callback))
                                callback(data);
                        })
                            .fail(function (data) {
                            alert("CSV Upload Failed");
                        })
                            .always(function () {
                            self.load();
                            intellitect.utilities.hideBusy();
                        });
                    }
                    // Remove the form
                    $('#csv-upload').remove();
                });
                // Click on the input box
                $('#csv-upload input[type=file]').click();
            };
            this.loadTimeout = 0;
            // reloads the page after a slight delay (100ms default) to ensure that all changes are made.
            this.delayedLoad = function (milliseconds) {
                if (_this.loadTimeout) {
                    clearTimeout(_this.loadTimeout);
                }
                _this.loadTimeout = setTimeout(function () {
                    _this.loadTimeout = 0;
                    _this.load();
                }, milliseconds || 100);
            };
            var searchTimeout = 0;
            this.pageSize.subscribe(function () {
                if (_this.isLoaded()) {
                    _this.load();
                }
            });
            this.page.subscribe(function () {
                if (_this.isLoaded() && !_this.isLoading()) {
                    _this.load();
                }
            });
            this.search.subscribe(function () {
                if (searchTimeout) {
                    clearTimeout(searchTimeout);
                }
                searchTimeout = setTimeout(function () {
                    searchTimeout = 0;
                    _this.load();
                }, 300);
            });
            this.orderBy.subscribe(function () { _this.delayedLoad(); });
            this.orderByDescending.subscribe(function () { _this.delayedLoad(); });
        }
        ;
        ;
        return BaseListViewModel;
    }());
    ListViewModels.BaseListViewModel = BaseListViewModel;
})(ListViewModels || (ListViewModels = {}));

/// <reference path="../coalesce.dependencies.d.ts" />


module Coalesce {

    export interface EnumValue {
        id: number;
        value: string;
    }

    interface ComputedConfiguration<T> extends KnockoutComputed<T> {
        raw: () => T
    };

    interface CoalesceConfiguration {
        [prop: string]: ComputedConfiguration<any> | any | undefined;
    }

    class CoalesceConfiguration {
        protected parentConfig?: CoalesceConfiguration;

        constructor(parentConfig?: CoalesceConfiguration) {
            this.parentConfig = parentConfig;
        }

        protected prop = function <TProp>(this: CoalesceConfiguration, name: string): ComputedConfiguration<TProp> {
            const k = "_" + name;
            const raw = this[k] = ko.observable<TProp>(null);
            var computed: ComputedConfiguration<TProp>;
            computed = ko.computed<TProp>({
                deferEvaluation: true, // This is essential. If not deferred, the observable will be immediately evaluated without parentConfig being set.
                read: () => {
                    var rawValue = raw();
                    if (rawValue !== null) return rawValue;

                    if (this.parentConfig && this.parentConfig[name as string]) {
                        return this.parentConfig[name as string]();
                    }

                    return null;
                },
                write: raw
            }) as any as ComputedConfiguration<TProp>;
            computed.raw = raw;
            return computed;
        }

        /**
            Gets the underlying observable that stores the object's explicit configuration value.
        */
        public raw = (name: Extract<keyof this, string>): KnockoutObservable<any> | undefined => {
            return (this as any)["_" + name];
        }
    }

    class ModelConfiguration<T> extends CoalesceConfiguration {
        /** The relative url where the API may be found. */
        public baseApiUrl = this.prop<string>("baseApiUrl");
        /** The relative url where the generated views may be found. */
        public baseViewUrl = this.prop<string>("baseViewUrl");
        /** Whether or not the callback specified for onFailure will be called or not. */
        public showFailureAlerts = this.prop<boolean>("showFailureAlerts");

        /** A callback to be called when a failure response is received from the server. */
        public onFailure = this.prop<(object: T, message: string) => void>("onFailure");
        /** A callback to be called when an AJAX request begins. */
        public onStartBusy = this.prop<(object: T) => void>("onStartBusy");
        /** A callback to be called when an AJAX request completes. */
        public onFinishBusy = this.prop<(object: T) => void>("onFinishBusy");
    }

    export class ViewModelConfiguration<T extends BaseViewModel> extends ModelConfiguration<T> {
        /** Time to wait after a change is seen before auto-saving (if autoSaveEnabled is true). Acts as a debouncing timer for multiple simultaneous changes. */
        public saveTimeoutMs = this.prop<number>("saveTimeoutMs");

        /** Determines whether changes to a model will be automatically saved after saveTimeoutMs milliseconds have elapsed. */
        public autoSaveEnabled = this.prop<boolean>("autoSaveEnabled");

        /** Determines whether or not changes to many-to-many collection properties will automatically trigger a save call to the server or not. */
        public autoSaveCollectionsEnabled = this.prop<boolean>("autoSaveCollectionsEnabled");

        /** Whether to invoke onStartBusy and onFinishBusy during saves. */
        public showBusyWhenSaving = this.prop<boolean>("showBusyWhenSaving");

        /** Whether or not to reload the ViewModel with the state of the object received from the server after a call to .save(). */
        public loadResponseFromSaves = this.prop<boolean>("loadResponseFromSaves");

        /**
            Whether or not to reload the ViewModel with the state of the object recieved from the server after a call to .deleteItem().
            This only applies to delete calls which respond with an object, which can be done through the model's behaviors.
        */
        public loadResponseFromDeletes = this.prop<boolean>("loadResponseFromDeletes");

        /**
            Whether or not the object should be removed from its parent after a call to /delete is made where the object is returned in the response.
            If no object is recieved from a /delete call, this option has no effect - it will always be removed from its parent in these cases.
        */
        public removeFromParentAfterSoftDelete = this.prop<boolean>("removeFromParentAfterSoftDelete");

        /**
            Whether or not to validate the model after loading it from a DTO from the server.
            Disabling this can improve performance in some cases.
        */
        public validateOnLoadFromDto = this.prop<boolean>("validateOnLoadFromDto");

        /**
            Whether or not validation on a ViewModel should be setup in its constructor,
            or if validation must be set up manually by calling viewModel.setupValidation().
            Turning this off can improve performance in read-only scenarios.
        */
        public setupValidationAutomatically = this.prop<boolean>("setupValidationAutomatically");

        /**
            An optional callback to be called when an object is loaded from a response from the server.
            Callback will be called after all properties on the ViewModel have been set from the server response.
        */
        public onLoadFromDto = this.prop<(object: T) => void>("onLoadFromDto");

        /**
            The dataSource (either an instance or a type) that will be used as the initial
            dataSource when a new object of this type is created.
            Not valid for global configuration; recommended to be used on class-level configuration.
            E.g. ViewModels.MyModel.coalesceConfig.initialDataSource(MyModel.dataSources.MyDataSource);
        */
        public initialDataSource = this.prop<DataSource<T> | (new () => DataSource<T>)>("initialDataSource");
    }

    export class ListViewModelConfiguration<T extends BaseListViewModel<TItem>, TItem extends BaseViewModel> extends ModelConfiguration<T> {
    }

    export class ServiceClientConfiguration<T extends ServiceClient> extends ModelConfiguration<T> {
    }

    export class AppConfiguration extends CoalesceConfiguration {

        /**
            A theme to specify on select2 instances created by Coalesce's select2-based bindings.
        */
        public select2Theme = this.prop<string | null>("select2Theme");
    }

    class RootConfig extends ModelConfiguration<any> {
        /** Application-wide configuration that does not pertain to any models. */
        public readonly app = new AppConfiguration();
        public readonly viewModel = new ViewModelConfiguration<BaseViewModel>(this);
        public readonly listViewModel = new ListViewModelConfiguration<BaseListViewModel<BaseViewModel>, BaseViewModel>(this);
        public readonly serviceClient = new ServiceClientConfiguration<ServiceClient>(this);
    }

    const invalidPropFunc: () => any = function () { if (arguments.length) throw "property is not valid at this level"; return null; };
    const invalidProp: any = invalidPropFunc;
    invalidProp.raw = invalidProp;

    export const GlobalConfiguration = new RootConfig();
    GlobalConfiguration.app.select2Theme(null);

    GlobalConfiguration.baseApiUrl("/api");
    GlobalConfiguration.baseViewUrl("");
    GlobalConfiguration.showFailureAlerts(true);
    GlobalConfiguration.onFailure((obj, message) => alert(message));
    GlobalConfiguration.onStartBusy(obj => Coalesce.Utilities.showBusy());
    GlobalConfiguration.onFinishBusy(obj => Coalesce.Utilities.hideBusy());

    GlobalConfiguration.viewModel.saveTimeoutMs(500);
    GlobalConfiguration.viewModel.autoSaveEnabled(true);
    GlobalConfiguration.viewModel.autoSaveCollectionsEnabled(true);
    GlobalConfiguration.viewModel.showBusyWhenSaving(false);
    GlobalConfiguration.viewModel.loadResponseFromSaves(true);
    GlobalConfiguration.viewModel.loadResponseFromDeletes(true);
    GlobalConfiguration.viewModel.removeFromParentAfterSoftDelete(false);
    GlobalConfiguration.viewModel.validateOnLoadFromDto(true);
    GlobalConfiguration.viewModel.setupValidationAutomatically(true);
    GlobalConfiguration.viewModel.initialDataSource = invalidProp;

    ko.validation.init({
        grouping: {
            deep: false,
            live: true,
            observable: true
        }
    });

    export interface LoadableViewModel {
        loadFromDto: (data: object) => void;
        parent: object | null;
        parentCollection: object | null;
    }

    export interface ClientMethodParent {
        coalesceConfig: ModelConfiguration<this>;
        apiController: string;
    }

    export interface ApiResult {
        wasSuccessful: boolean;
        message?: string;
    }

    export interface ValidationIssue {
        property: string;
        issue: string;
    }

    export interface ItemResult<T = any> extends ApiResult {
        object?: T;
        validationIssues?: ValidationIssue[];
    }

    export interface ListResult extends ApiResult {
        list?: any[];
        page: number;
        pageSize: number;
        pageCount: number;
        totalCount: number;
    }

    export abstract class ClientMethod<TParent extends ClientMethodParent, TResult> {
        public abstract readonly name: string;

        /** HTTP method to be used when calling the API endpoint. */
        public readonly verb: string = "POST";

        /** Result of method strongly typed in a observable. */
        public result: KnockoutObservable<TResult> = ko.observable<TResult>(null);

        /** Raw result object of method simply wrapped in an observable. */
        public rawResult: KnockoutObservable<ItemResult | null> = ko.observable(null);

        /** True while the method is being called */
        public isLoading: KnockoutObservable<boolean> = ko.observable<boolean>(false);

        /** Error response when method has failed. */
        public message: KnockoutObservable<string> = ko.observable<string>(null);

        /** True if last invocation of method was successful. */
        public wasSuccessful: KnockoutObservable<boolean | null> = ko.observable(null);

        constructor(protected parent: TParent) { }

        protected abstract loadResponse: (data: ApiResult, callback?: (result: TResult) => void, reload?: boolean) => void;

        protected loadStandardResponse = (data: ApiResult) => { /* Nothing, normally. Other abstract derived classes can use this for non-specific result loading. */ };

        protected invokeWithData(postData: object, callback?: (result: TResult) => void, reload?: boolean) {
            this.isLoading(true);
            this.message('');
            this.wasSuccessful(null);
            return $.ajax({
                method: this.verb,
                url: this.parent.coalesceConfig.baseApiUrl() + this.parent.apiController + '/' + this.name,
                data: postData,
                xhrFields: { withCredentials: true }
            })
                .done((data: ApiResult) => {
                    // This is here because it was migrated from the old client method calls.
                    // Whether or not this should be done remains to be see, but it was kept to reduce
                    // the number of breaking changes being made.
                    if (this.parent instanceof BaseViewModel)
                        this.parent.isDirty(false);

                    this.rawResult(data);
                    this.message('');
                    this.wasSuccessful(true);
                    this.loadStandardResponse(data);
                    this.loadResponse(data, callback, reload);
                })
                .fail((xhr) => {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    this.wasSuccessful(false);
                    this.message(errorMsg);

                    if (this.parent.coalesceConfig.showFailureAlerts()) {
                        this.parent.coalesceConfig.onFailure()(this.parent, `Could not call method ${this.name}: ${errorMsg}`);
                    }
                })
                .always(() => {
                    this.isLoading(false);
                });
        }
    }

    export abstract class ClientListMethod<TParent extends ClientMethodParent, TResult> extends ClientMethod<TParent, TResult> {
        /** Page number. */
        public page: KnockoutObservable<number | null> = ko.observable(null);
        /** Number of items on a page. */
        public pageSize: KnockoutObservable<number | null> = ko.observable(null);
        /** Total count of items, even ones that are not on the page. */
        public totalCount: KnockoutObservable<number | null> = ko.observable(null);
        /** Total page count */
        public pageCount: KnockoutObservable<number | null> = ko.observable(null);

        /** Raw result object of method simply wrapped in an observable. */
        public rawResult: KnockoutObservable<ListResult | null> = ko.observable(null);

        protected loadStandardResponse = (data: ApiResult) => {
            const listResult = data as ListResult;
            this.page(listResult.page);
            this.pageSize(listResult.pageSize);
            this.totalCount(listResult.totalCount);
            this.pageCount(listResult.pageCount);
        }
    }

    export abstract class DataSource<T extends BaseViewModel> {
        protected _name: string;

        public saveToDto: () => { [x: string]: string } = () => { return {}; }

        // This is computed so we can subscribe to when the request to the server changes,
        // and then reload objects/lists accordingly.
        public getQueryString = ko.computed(() => {
            var query = `dataSource=${this._name}`;

            //&${$.param({ dataSource: this.saveToDto() }).replace(/dataSource%5B(.*?)%5D/g, 'dataSource.$1')}

            var dto = this.saveToDto();
            for (var key in dto) {
                if (dto[key] !== null && dto[key] !== undefined) {
                    query += `&dataSource.${key}=${encodeURIComponent(dto[key])}`
                }
            }
            return query;
        }, null, { deferEvaluation: true });

        /**
            Subscribe the given list to changes in the data source's parameters,
            triggering a reload upon changed parameter values.
        */
        public subscribe = (list: BaseListViewModel<T>) => {
            this.getQueryString.subscribe(() => {
                if (list.isLoaded()) {
                    list.delayedLoad(300);
                }
            })
        }

        constructor() {
            this._name = Coalesce.Utilities.getClassName(this);
        }
    }

    export abstract class ServiceClient {
        public readonly abstract apiController: string;
    }

    export abstract class BaseViewModel implements LoadableViewModel, ClientMethodParent {

        public readonly abstract modelName: string;
        public readonly abstract modelDisplayName: string;

        // Typing this property as keyof this prevents us from using BaseViewModel amorphously.
        // It prevents assignment of an arbitrary derived type to a variable/parameter expecting BaseViewModel
        // because primaryKeyName on a derived type is wider than it is on BaseViewModel.
        public readonly abstract primaryKeyName: string;

        public readonly abstract apiController: string;
        public readonly abstract viewController: string;

        /**
            List of all possible data sources that can be set on the dataSource property.
        */
        public abstract dataSources: any;

        /**
            The custom data source that will be invoked on the server to provide the data for this list.
        */
        public dataSource: DataSource<this>;

        /**
            Properties which determine how this object behaves.
        */
        public abstract coalesceConfig: ViewModelConfiguration<this>;

        /** Stack for number of times loading has been called. */
        protected loadingCount: number = 0;
        /** Stores the return value of setInterval for automatic save delays. */
        protected saveTimeout: number = 0;


        /** Callbacks to call after a save. */
        protected saveCallbacks: Array<(self: this) => void> = [];

        /**
            String that will be passed to the server when loading and saving that allows for data trimming via C# Attributes.
        */
        public includes: string = "";

        /**
            If true, the busy indicator is shown when loading.
            @deprecated Use coalesceConfig.showBusyWhenSaving instead.
        */
        get showBusyWhenSaving() { return this.coalesceConfig.showBusyWhenSaving() }
        set showBusyWhenSaving(value) { this.coalesceConfig.showBusyWhenSaving(value) }

        /**
            Whether or not alerts should be shown when loading fails.
            @deprecated Use coalesceConfig.showFailureAlerts instead.
        */
        get showFailureAlerts() { return this.coalesceConfig.showFailureAlerts() }
        set showFailureAlerts(value) { this.coalesceConfig.showFailureAlerts(value) }

        /** Parent of this object, if this object was loaded as part of a hierarchy. */
        public parent: BaseViewModel | BaseListViewModel<this> | null;
        /** Parent of this object, if this object was loaded as part of list of objects. */
        public parentCollection: KnockoutObservableArray<this> | null = null;
        /**
            Primary Key of the object.
            @deprecated Use the strongly-typed property of the key for this model whenever possible. This property will be removed once Coalesce supports composite keys.
        */
        public myId: any = 0;

        /** Dirty Flag. Set when a value on the model changes. Reset when the model is saved or reloaded. */
        public isDirty: KnockoutObservable<boolean> = ko.observable(false);
        /** Contains the error message from the last failed call to the server. */
        public errorMessage: KnockoutObservable<string | null> = ko.observable(null);

        /**
            If this is true, all changes will be saved automatically.
            @deprecated Use coalesceConfig.autoSaveEnabled instead.
        */
        get isSavingAutomatically() { return this.coalesceConfig.autoSaveEnabled() }
        set isSavingAutomatically(value) { this.coalesceConfig.autoSaveEnabled(value) }


        /** Flag to use to determine if this item is shown. Provided for convenience. */
        public isVisible: KnockoutObservable<boolean> = ko.observable(false);
        /** Flag to use to determine if this item is expanded. Provided for convenience. */
        public isExpanded: KnockoutObservable<boolean> = ko.observable(false);
        /** Flag to use to determine if this item is selected. Provided for convenience. */
        public isSelected: KnockoutObservable<boolean> = ko.observable(false);
        /** Flag to use to determine if this item is checked. Provided for convenience. */
        public isChecked: KnockoutObservable<boolean> = ko.observable(false);
        /** Flag to use to determine if this item is being edited. Provided for convenience. */
        public isEditing: KnockoutObservable<boolean> = ko.observable(false);

        /** Toggles the isExpanded flag. Use with a click binding for a button. */
        public toggleIsExpanded = (): void => this.isExpanded(!this.isExpanded());

        /** Toggles the isEditing flag. Use with a click binding for a button. */
        public toggleIsEditing = (): void => this.isEditing(!this.isEditing());

        /** Toggles the isSelected flag. Use with a click binding for a button. */
        public toggleIsSelected = (): void => this.isSelected(!this.isSelected());

        /**
            Sets isSelected(true) on this object and clears on the rest of the items in the parent collection.
            @returns true to bubble additional click events.
        */
        public selectSingle = (): boolean => {
            if (this.parentCollection && this.parentCollection()) {
                $.each(this.parentCollection(), (i, obj) => {
                    obj.isSelected(false);
                });
            }
            this.isSelected(true);
            return true; // Allow other click events
        };


        /** List of errors found during validation. Any errors present will prevent saving. */
        public errors: KnockoutValidationErrors | null = null;
        /** List of warnings found during validation. Saving is still allowed with warnings present. */
        public warnings: KnockoutValidationErrors | null = null;

        /** True if the object is currently saving. */
        public isSaving: KnockoutObservable<boolean> = ko.observable(false);
        /** Internal count of child objects that are saving. */
        protected savingChildCount: KnockoutObservable<number> = ko.observable(0);

        /** 
            Returns true if there are no client-side validation issues.
            Saves will be prevented if this returns false.
        */
        public isValid = (): boolean => this.errors == null || this.errors().length == 0;

        /**
            Triggers any validation messages to be shown, and returns a bool that indicates if there are any validation errors.
        */
        public validate = (): boolean => {
            if (this.errors) this.errors.showAllMessages();
            if (this.warnings) this.warnings.showAllMessages();
            return this.isValid();
        };

        /** Setup knockout validation on this ViewModel. This is done automatically unless disabled with setupValidationAutomatically(false) */
        public abstract setupValidation(): void;

        /** True if the object is loading. */
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);
        /**  True once the data has been loaded. */
        public isLoaded: KnockoutObservable<boolean> = ko.observable(false);
        /** URL to a stock editor for this object. */
        public editUrl: KnockoutComputed<string> = ko.pureComputed(() => {
            return this.coalesceConfig.baseViewUrl() + this.viewController + "/CreateEdit?id=" + ((this as any)[this.primaryKeyName])();
        });


        /**
          * Loads this object from a data transfer object received from the server.
          * @param force - Will override the check against isLoading that is done to prevent recursion.
          * @param allowCollectionDeletes - Set true when entire collections are loaded. True is the default.
                In some cases only a partial collection is returned, set to false to only add/update collections.
        */
        public abstract loadFromDto: (data: any, force?: boolean, allowCollectionDeletes?: boolean) => void;

        /** Saves this object into a data transfer object to send to the server. */
        public abstract saveToDto: () => any;

        /**
            Loads any child objects that have an ID set, but not the full object.
            This is useful when creating an object that has a parent object and the ID is set on the new child.
        */
        public abstract loadChildren: (callback?: () => void) => void;


        /** Returns true if the current object, or any of its children, are saving. */
        public isThisOrChildSaving: KnockoutComputed<boolean> = ko.computed(() => {
            if (this.isSaving()) return true;
            if (this.savingChildCount() > 0) return true;
            return false;
        });

        // Handle children that are saving.
        // Internally used member to count the number of saving children.
        protected onSavingChildChange = (isSaving: boolean): void => {
            if (isSaving)
                this.savingChildCount(this.savingChildCount() + 1);
            else
                this.savingChildCount(this.savingChildCount() - 1);

            if (this.parent instanceof BaseViewModel) {
                this.parent.onSavingChildChange(isSaving);
            }
        };

        /**
            Saves the object to the server and then calls a callback.
            Returns false if there are validation errors.
        */
        public save = (callback?: (self: this) => void): JQueryPromise<any> | boolean | undefined => {
            if (!this.isLoading()) {
                if (this.validate()) {
                    if (this.coalesceConfig.showBusyWhenSaving()) this.coalesceConfig.onStartBusy()(this);
                    this.cancelAutoSave();
                    this.isSaving(true);
                    var url = `${this.coalesceConfig.baseApiUrl()}${this.apiController}/Save?includes=${this.includes}&${this.dataSource.getQueryString()}`
                    return $.ajax({ method: "POST", url: url, data: this.saveToDto(), xhrFields: { withCredentials: true } })
                        .done((data: ItemResult) => {
                            this.isDirty(false);
                            this.errorMessage(null);
                            if (this.coalesceConfig.loadResponseFromSaves()) {
                                this.loadFromDto(data.object, true);
                            }
                            // The object is now saved. Call any callback.
                            for (var i in this.saveCallbacks) {
                                this.saveCallbacks[i](this);
                            }
                        })
                        .fail((xhr: JQueryXHR) => {
                            var errorMsg = "Unknown Error";
                            const data: ItemResult | null = xhr.responseJSON
                            if (data && data.message) errorMsg = data.message;
                            this.errorMessage(errorMsg);

                            // If an object was returned, load that object.
                            if (data && data.object) {
                                this.loadFromDto(data.object, true);
                            }
                            if (this.coalesceConfig.showFailureAlerts())
                                this.coalesceConfig.onFailure()(this, "Could not save the item: " + errorMsg);
                        })
                        .always(() => {
                            this.isSaving(false);
                            if (typeof(callback) == "function") {
                                callback(this);
                            }
                            if (this.coalesceConfig.showBusyWhenSaving()) this.coalesceConfig.onFinishBusy()(this);
                        });
                }
                else {
                    // If validation fails, we still want to try and load any child objects which may have just been set.
                    // Normally, we get these from the result of the save.
                    this.loadChildren();
                    return false;
                }
            }
        }


        /** Loads the object from the server based on the id specified. If no id is specified, the current id is used if one is set. */
        public load = (id?: any, callback?: ((self: this) => void) | null): JQueryPromise<any> | undefined => {
            if (!id) {
                id = ((this as any)[this.primaryKeyName])();
            }
            if (id) {
                this.isLoading(true);
                this.coalesceConfig.onStartBusy()(this);

                var url = `${this.coalesceConfig.baseApiUrl()}${this.apiController}/Get/${id}?includes=${this.includes}&${this.dataSource.getQueryString()}`

                return $.ajax({ method: "GET", url: url, xhrFields: { withCredentials: true } })
                    .done((data: ItemResult) => {
                        this.errorMessage(null);
                        this.loadFromDto(data.object, true);
                        this.isLoaded(true);
                        if (typeof(callback) == "function") callback(this);
                    })
                    .fail((xhr: JQueryXHR) => {
                        const data: ItemResult | null = xhr.responseJSON
                        var errorMsg = "Could not load " + this.modelName + " with ID = " + id;
                        if (data && data.message) errorMsg = data.message;

                        this.errorMessage(errorMsg);
                        if (this.coalesceConfig.showFailureAlerts())
                            this.coalesceConfig.onFailure()(this, errorMsg);
                    })
                    .always(() => {
                        this.coalesceConfig.onFinishBusy()(this);
                        this.isLoading(false);
                    });
            }
        };

        /** Deletes the object without any prompt for confirmation. */
        public deleteItem = (callback?: (self: this) => void): JQueryPromise<any> | undefined => {
            var currentId = ((this as any)[this.primaryKeyName])();
            if (currentId) {
                return $.ajax({ method: "POST", url: this.coalesceConfig.baseApiUrl() + this.apiController + "/Delete/" + currentId, xhrFields: { withCredentials: true } })
                    .done((data: ItemResult) => {
                        this.errorMessage(null);

                        if (data.object != null && this.coalesceConfig.loadResponseFromDeletes()) {
                            this.loadFromDto(data.object, true);
                        }

                        // Remove it from the parent collection
                        if (this.parentCollection && this.parent) {
                            var shouldRemoveFromParent = (data.object == null || this.coalesceConfig.removeFromParentAfterSoftDelete());
                            if (!shouldRemoveFromParent) {
                                // be a Good Citizen and tell the user why the item they just deleted wasn't removed from the parent collection, as this isn't always super intuitive.
                                console.warn("Deleted item was not removed from its parent because the API call returned an object and this.coalesceConfig.removeFromParentAfterSoftDelete() == false")
                            } else {
                                this.parent.isLoading(true);
                                this.parentCollection.splice(this.parentCollection().indexOf(this), 1);
                                this.parent.isLoading(false);
                            }
                        }
                    })
                    .fail((xhr: JQueryXHR) => {
                        var errorMsg = "Could not delete the item";
                        const data: ItemResult | null = xhr.responseJSON
                        if (data && data.message) errorMsg = data.message;

                        this.errorMessage(errorMsg);
                        if (this.coalesceConfig.showFailureAlerts())
                            this.coalesceConfig.onFailure()(this, errorMsg);
                    })
                    .always(() => {
                        if (typeof(callback) == "function") {
                            callback(this);
                        }
                    });
            } else {
                // No ID has been assigned yet, just remove it.
                if (this.parentCollection && this.parent) {
                    this.parent.isLoading(true);
                    this.parentCollection.splice(this.parentCollection().indexOf(this), 1);
                    this.parent.isLoading(false);
                }
                if (typeof(callback) == "function") {
                    callback(this);
                }
            }
        };

        /**
            Deletes the object if a prompt for confirmation is answered affirmatively.
        */
        public deleteItemWithConfirmation = (callback?: () => void, message?: string): JQueryPromise<any> | undefined => {
            if (typeof message != 'string') {
                message = "Delete this item?";
            }
            if (confirm(message)) {
                return this.deleteItem(callback);
            }
        };

        /** Saves a many-to-many collection change. This is done automatically and doesn't need to be called. */
        protected saveCollection = <TJoin extends BaseViewModel>(
            operation: 'added' | 'deleted',
            existingItems: KnockoutObservableArray<TJoin>,
            constructor: new () => TJoin,
            localIdProp: keyof TJoin,
            foreignIdProp: keyof TJoin,
            foreignId: any
        ): JQueryPromise<any> | boolean | undefined => {

            var currentId = ((this as any)[this.primaryKeyName])();

            if (operation == 'added') {
                var newItem = new constructor();
                newItem.parent = this;
                newItem.parentCollection = existingItems;
                newItem.coalesceConfig.autoSaveEnabled(false);
                (newItem[localIdProp] as any)(currentId);
                (newItem[foreignIdProp] as any)(foreignId);
                return newItem.save(() => {
                    // Restore default autosave behavior.
                    newItem.coalesceConfig.autoSaveEnabled(null);
                    existingItems.push(newItem);
                });
            } else if (operation == 'deleted') {
                var matchedItems = existingItems().filter((i: any) => i[localIdProp]() === currentId && i[foreignIdProp]() === foreignId);
                if (matchedItems.length == 0) {
                    throw `Couldn't find a ${constructor.toString()} object to delete with ${localIdProp}=${currentId} & ${foreignIdProp}=${foreignId}.`
                } else {
                    // If we matched more than one item, we're just going to operate on the first one.
                    var matcheditem = matchedItems[0];
                    return matcheditem.deleteItem();
                }
            }
        };

        /** Saves a many to many collection if coalesceConfig.autoSaveCollectionsEnabled is true. */
        protected autoSaveCollection = <TJoin extends BaseViewModel>(
            operation: string,
            existingItems: KnockoutObservableArray<TJoin>,
            constructor: new () => TJoin,
            localIdProp: keyof TJoin,
            foreignIdProp: keyof TJoin,
            foreignId: any
        ) => {
            if (!this.isLoading() && this.coalesceConfig.autoSaveCollectionsEnabled()) {

                // TODO: Eventually Batch saves for many-to-many collections.
                if (operation != 'added' && operation != 'deleted') return;

                this.saveCollection<TJoin>(operation, existingItems, constructor, localIdProp, foreignIdProp, foreignId);
            }
        };


        /**
            Register a callback to be called when a save is done.
            @returns true if the callback was registered. false if the callback was already registered. 
        */
        public onSave = (callback: (self: this) => void): boolean => {
            if (typeof(callback) == "function" && !this.saveCallbacks.filter(c => c == callback).length) {
                this.saveCallbacks.push(callback);
                return true;
            }
            return false;
        };

        /** Saves the object is coalesceConfig.autoSaveEnabled is true. */
        public autoSave = (): void => {
            if (!this.isLoading()) {
                this.isDirty(true);
                if (this.coalesceConfig.autoSaveEnabled()) {
                    // Batch saves.
                    this.cancelAutoSave();
                    this.saveTimeout = setTimeout(() => {
                        // If we have a save in progress, wait...
                        if (this.isSaving()) {
                            this.autoSave();
                        } else if (this.coalesceConfig.autoSaveEnabled()) {
                            this.save();
                        }
                    }, this.coalesceConfig.saveTimeoutMs());
                }
            }
        }

        /** Cancels a pending autosave if it exists. */
        public cancelAutoSave = (): void => {
            if (this.saveTimeout) {
                clearTimeout(this.saveTimeout);
                this.saveTimeout = 0;
            }
        }

        /**
            Displays an editor for the object in a modal dialog.
        */
        public showEditor = (callback?: any): JQueryPromise<any> => {
            // Close any existing modal
            $('#modal-dialog').modal('hide');
            // Get new modal content
            this.coalesceConfig.onStartBusy()(this);
            return $.ajax({
                method: "GET",
                url: this.coalesceConfig.baseViewUrl() + this.viewController + '/EditorHtml',
                data: { simple: true },
                xhrFields: { withCredentials: true }
            })
                .done((data) => {
                    // Add to DOM
                    Coalesce.ModalHelpers.setupModal('Edit ' + this.modelDisplayName, data, true, false);
                    // Data bind
                    var lastValue = this.coalesceConfig.autoSaveEnabled.raw();
                    this.coalesceConfig.autoSaveEnabled(false);
                    ko.applyBindings(this, document.getElementById("modal-dialog"));
                    this.coalesceConfig.autoSaveEnabled(lastValue);
                    // Show the dialog
                    $('#modal-dialog').modal('show');
                    // Make the callback when the form closes.
                    $("#modal-dialog").on("hidden.bs.modal", () => {
                        if (typeof(callback) == "function") callback(this);
                    });
                })
                .always(() => {
                    this.coalesceConfig.onFinishBusy()(this);
                });
        }

        /**
            Common base-class level initialization that depends on all constructors being ran
            (and therefore cannot be performed directly in the base constructor).
        */
        protected baseInitialize() {

            var dataSource = this.coalesceConfig.initialDataSource.peek();
            if (dataSource === null) {
                this.dataSource = new this.dataSources.Default()
            } else {
                if (dataSource instanceof Coalesce.DataSource) {
                    this.dataSource = dataSource
                } else {
                    this.dataSource = new dataSource();
                }
            }

            if (this.coalesceConfig.setupValidationAutomatically.peek()) {
                this.setupValidation();
            }
        }

        constructor(parent?: Coalesce.BaseViewModel | Coalesce.BaseListViewModel<any> | null) {
            this.parent = parent || null;

            // Handles setting the parent savingChildChange
            this.isSaving.subscribe((newValue: boolean) => {
                if (this.parent instanceof BaseViewModel) {
                    this.parent.onSavingChildChange(newValue);
                }
            })
        }
    }

    export abstract class BaseListViewModel<TItem extends BaseViewModel> implements ClientMethodParent {

        public readonly abstract modelName: string;
        public readonly abstract apiController: string;

        /**
            List of all possible data sources that can be set on the dataSource property.
        */
        public abstract dataSources: any;

        /**
            The custom data source that will be invoked on the server to provide the data for this list.
        */
        public abstract dataSource: DataSource<TItem>;

        /**
            Name of the primary key of the model that this list represents.
        */
        public abstract modelKeyName: string;

        // Reference to the class which this list represents.
        protected abstract itemClass: new() => TItem;

        /**
            Properties which determine how this object behaves.
        */
        public abstract coalesceConfig: ListViewModelConfiguration<this, TItem>;

        /**
            Query string to append to the API call when loading the list of items.
        */
        public queryString: string = "";

        /**
            Object that contains property-level filters to be passed along to API calls.
        */
        public filter: any = null;

        /** String that is used to control loading and serialization on the server. */
        public includes: string = "";

        /**
            Whether or not alerts should be shown when loading fails.
            @deprecated Use coalesceConfig.showFailureAlerts instead.
        */
        get showFailureAlerts() { return this.coalesceConfig.showFailureAlerts() }
        set showFailureAlerts(value) { this.coalesceConfig.showFailureAlerts(value) }

        /** The collection of items that have been loaded from the server. */
        public items: KnockoutObservableArray<TItem> = ko.observableArray([]);

        /**
            Load the list using current parameters for paging, searching, etc
            Result is placed into the items property.
        */
        public load = (callback?: any): JQueryPromise<any> => {
            this.coalesceConfig.onStartBusy()(this);
            this.isLoading(true);

            var url = this.coalesceConfig.baseApiUrl() + this.apiController + "/List?" + this.queryParams('list');

            return $.ajax({
                method: "GET",
                url: url,
                xhrFields: { withCredentials: true }
            })
                .done((data: ListResult) => {
                    const list = data.list || [];

                    Coalesce.KnockoutUtilities.RebuildArray(this.items, list, this.modelKeyName, this.itemClass, this, true);
                    $.each(this.items(), (_, model) => {
                        model.includes = this.includes;
                    });
                    this.count(list.length);
                    this.totalCount(data.totalCount);
                    this.pageCount(data.pageCount);
                    this.page(data.page);
                    this.message(typeof(data.message) == "string" ? data.message : null);
                    this.isLoaded(true);
                    if (typeof(callback) == "function") callback(this);
                })
                .fail((xhr) => {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    this.message(errorMsg);

                    if (this.coalesceConfig.showFailureAlerts())
                        this.coalesceConfig.onFailure()(this, "Could not get list of " + this.modelName + " items: " + errorMsg);
                })
                .always(() => {
                    this.coalesceConfig.onFinishBusy()(this);
                    this.isLoading(false);
                });
        };


        /** Returns a query string built from the list's various properties, appropriate to the kind of parameters requested. */
        public queryParams = (kind: 'dataSource' | 'filter' | 'list', pageSize?: number): string => {
            var query = this.dataSource.getQueryString();

            const param = (name: string, value: any) => {
                if (value === null || value === undefined || value === "") {
                    return;
                }
                query += `&${name}=${encodeURIComponent(value)}`;
            };

            param("includes", this.includes);

            if (kind == 'dataSource') return query;

            if (this.queryString) query += "&" + this.queryString;

            param("search", this.search());

            if (this.filter) {
                for (var key in this.filter) {
                    param(`filter.${key}`, this.filter[key]);
                }
            }

            if (kind == 'filter') return query;

            if (kind != 'list') throw "unhandled kind " + kind;

            param(`page`, this.page());
            param(`pageSize`, pageSize || this.pageSize());
            param(`orderBy`, this.orderBy());
            param(`orderByDescending`, this.orderByDescending());

            return query;
        };

        /** Method which will instantiate a new item of the list's model type. */
        protected abstract createItem: (newItem?: any, parent?: any) => TItem;

        /** Adds a new item to the collection. */
        public addNewItem = (): TItem => {
            var item = this.createItem();
            this.items.push(item);
            return item;
        };

        /** Deletes an item. */
        public deleteItem = (item: TItem): JQueryPromise<any> | undefined => {
            return item.deleteItem();
        };

        /** True if the list is loading. */
        public isLoading: KnockoutObservable<boolean> = ko.observable(false);

        /** True once the list has been loaded. */
        public isLoaded: KnockoutObservable<boolean> = ko.observable(false);

        /** Gets the count of items without getting all the items. Result is placed into the count property. */
        public getCount = (callback?: any): JQueryPromise<any> => {
            this.coalesceConfig.onStartBusy()(this);
            return $.ajax({
                method: "GET",
                url: this.coalesceConfig.baseApiUrl() + this.apiController + "/Count?" + this.queryParams('filter'),
                xhrFields: { withCredentials: true }
            })
                .done((data: ItemResult<number>) => {
                    this.count(data.object || 0);
                    this.message(typeof(data.message) == "string" ? data.message : null);
                    if (typeof(callback) == "function") callback();
                })
                .fail((xhr) => {
                    var errorMsg = "Unknown Error";
                    var result: ItemResult<number> = xhr.responseJSON;
                    if (result && result.message) errorMsg = result.message;
                    this.message(errorMsg);
                    if (this.coalesceConfig.showFailureAlerts())
                        this.coalesceConfig.onFailure()(this, "Could not get count of " + this.modelName + " items: " + errorMsg);
                })
                .always(() => {
                    this.coalesceConfig.onFinishBusy()(this);
                });
        };

        /** The result of getCount() or the total on this page. */
        public count: KnockoutObservable<number | null> = ko.observable(null);
        /** Total count of items, even ones that are not on the page. */
        public totalCount: KnockoutObservable<number | null> = ko.observable(null);
        /** Total page count */
        public pageCount: KnockoutObservable<number | null> = ko.observable(null);
        /** Page number. This can be set to get a new page. */
        public page: KnockoutObservable<number> = ko.observable(1);
        /** Number of items on a page. */
        public pageSize: KnockoutObservable<number> = ko.observable(10);
        /** If a load failed, this is a message about why it failed. */
        public message: KnockoutObservable<string | null> = ko.observable(null);
        /** Search criteria for the list. This can be exposed as a text box for searching. */
        public search: KnockoutObservable<string> = ko.observable("");

        /** True if there is another page after the current page. */
        public nextPageEnabled: KnockoutComputed<boolean> = ko.computed(() => this.page() < (this.pageCount() || 0));

        /** True if there is another page before the current page. */
        public previousPageEnabled: KnockoutComputed<boolean> = ko.computed(() => this.page() > 1);

        /** Change to the next page */
        public nextPage = (): void => {
            if (this.nextPageEnabled()) {
                this.page(this.page() + 1);
            }
        };

        /** Change to the previous page */
        public previousPage = (): void => {
            if (this.previousPageEnabled()) {
                this.page(this.page() - 1);
            }
        };


        /** Name of a field by which this list will be loaded in ascending order */
        public orderBy: KnockoutObservable<string> = ko.observable("");

        /** Name of a field by which this list will be loaded in descending order */
        public orderByDescending: KnockoutObservable<string> = ko.observable("");

        /** Toggles sorting between ascending, descending, and no order on the specified field. */
        public orderByToggle = (field: string): void => {
            if (this.orderBy() == field && !this.orderByDescending()) {
                this.orderBy('');
                this.orderByDescending(field);
            } else if (!this.orderBy() && this.orderByDescending() == field) {
                this.orderBy('');
                this.orderByDescending('');
            } else {
                this.orderBy(field);
                this.orderByDescending('');
            }
        };

        /** Returns URL to download a CSV for the current list with all items. */
        public downloadAllCsvUrl: KnockoutComputed<string> = ko.computed<string>(() => {
            var url = this.coalesceConfig.baseApiUrl() + this.apiController + "/CsvDownload?" + this.queryParams('list', 10000);
            return url;
        }, null, { deferEvaluation: true });

        /** Prompts to the user for a file to upload as a CSV. */
        public csvUploadUi = (callback?: () => void): void => {
            // Remove the form if it exists.
            $('#csv-upload').remove();
            // Add the form to the page to take the input
            $('body')
                .append('<form id="csv-upload" display="none"></form>');
            $('#csv-upload')
                .attr("action", this.coalesceConfig.baseApiUrl() + this.apiController + "/CsvUpload").attr("method", "post")
                .append('<input type="file" style="visibility: hidden;" name="file"/>');

            // Set up the click callback.
            $('#csv-upload input[type=file]').change(() => {
                // Get the files
                var fileInput = $('#csv-upload input[type=file]')[0] as any;
                var file = fileInput.files[0];
                if (file) {
                    var formData = new FormData();
                    formData.append('file', file);
                    this.coalesceConfig.onStartBusy()(this);
                    this.isLoading(true);
                    $.ajax({
                        url: this.coalesceConfig.baseApiUrl() + this.apiController + "/CsvUpload",
                        data: formData,
                        processData: false,
                        contentType: false,
                        type: 'POST'
                    } as any)
                        .done((data) => {
                            this.isLoading(false);
                            if (typeof(callback) == "function") callback();
                        })
                        .fail((data) => {
                            if (this.coalesceConfig.showFailureAlerts())
                                this.coalesceConfig.onFailure()(this, "CSV Upload Failed");
                        })
                        .always(() => {
                            this.load();
                            this.coalesceConfig.onFinishBusy()(this);
                        });
                }
                // Remove the form
                $('#csv-upload').remove();
            });
            // Click on the input box
            $('#csv-upload input[type=file]').click();
        };

        private loadTimeout: number = 0;

        /** reloads the list after a slight delay (100ms default) to ensure that all changes are made. */
        public delayedLoad = (milliseconds?: number): void => {
            if (this.loadTimeout) {
                clearTimeout(this.loadTimeout);
            }
            this.loadTimeout = setTimeout(() => {
                this.loadTimeout = 0;
                this.load();
            }, milliseconds || 100);
        }

        public constructor() {
            this.pageSize.subscribe(() => {
                if (this.isLoaded()) {
                    this.load();
                }
            });
            this.page.subscribe(() => {
                // Page is set while we're loading results - ignore changes while isLoading() == true
                if (this.isLoaded() && !this.isLoading()) {
                    this.delayedLoad(300);
                }
            });
            this.search.subscribe(() => { if (this.isLoaded()) this.delayedLoad(300); });
            this.orderBy.subscribe(() => { if (this.isLoaded()) this.delayedLoad(); });
            this.orderByDescending.subscribe(() => { if (this.isLoaded()) this.delayedLoad(); });
        }
    }
}
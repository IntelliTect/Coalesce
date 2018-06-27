import debounce from 'lodash-es/debounce';
import { resolvePropMeta } from './metadata';
import { ListParameters } from './api-client';
import { modelDisplay, propDisplay, mapToDto, convertToModel, updateFromModel } from './model';
/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
export function defineProps(ctor, metadata) {
    Object.defineProperties(ctor.prototype, Object
        .keys(metadata.props)
        .reduce(function (descriptors, propName) {
        descriptors[propName] = {
            enumerable: true,
            get: function () {
                return this.$data[propName];
            },
            set: function (val) {
                this.$data[propName] = val;
            }
        };
        return descriptors;
    }, {}));
}
/*
DESIGN NOTES
    - ViewModel deliberately has TModel as its only type parameter.
        The type of the metadata is always accessed off of TModel as TModel["$metadata"].
        This makes the intellisense in IDEs quite nice. If TMeta is a type param,
        we end up with the type of implemented classes taking several pages of the intellisense tooltip.
        With this, we can still strongly type off of known information of TMeta (like PropNames<TModel["$metadata"]>),
        but without cluttering up tooltips with the entire type structure of the metadata.
    - ViewModels never instantiate other ViewModels on the users' behalf. ViewModels must always be instantiated explicitly.
        This makes it much easier to reason about the behavior of a program
        when Coalesce isn't creating ViewModel instances on the developers' behalf.
        It prevents the existance of deeply nested, difficult-to-access (or even find at all) instances
        that are difficult to configure. Ideally, all ViewModels exist on instances of components.
        This also allows subclassing of ViewModel classes at will because any place where a ViewModel
        is instantiated can be replaced with any other subclass of that ViewModel by the developer with ease.
*/
var ViewModel = /** @class */ (function () {
    function ViewModel(
    // The following MUST be declared in the constructor so its value will be available to property initializers.
    /** The metadata representing the type of data that this ViewModel handles. */
    $metadata, 
    /** Instance of an API client for the model through which direct, stateless API requests may be made. */
    $apiClient, initialData) {
        var _this = this;
        this.$metadata = $metadata;
        this.$apiClient = $apiClient;
        // Must be initialized so it will be reactive.
        // If this isn't reactive, $isDirty won't be reactive.
        // Technically this will always be initialized by the setting of `$isDirty` in the ctor.
        this._pristineDto = null;
        /**
         * A function for invoking the /get endpoint, and a set of properties about the state of the last call.
         */
        this.$load = this.$apiClient.$makeCaller("item", function (c) { return function (id) { return c.get(id != null ? id : _this.$primaryKey); }; })
            .onFulfilled(function () {
            if (_this.$load.result) {
                updateFromModel(_this.$data, _this.$load.result);
                _this.$isDirty = false;
            }
        });
        /**
         * A function for invoking the /save endpoint, and a set of properties about the state of the last call.
         */
        this.$save = this.$apiClient.$makeCaller("item", function (c) { return function () {
            // Before we make the save call, set isDirty = false.
            // This lets us detect changes that happen to the model while our save request is pending.
            // If the model is dirty when the request completes, we'll not load the response from the server.
            _this.$isDirty = false;
            return c.save(_this.$data);
        }; })
            .onFulfilled(function () {
            if (!_this.$save.result) {
                // Can't do anything useful if the save returned no data.
                return;
            }
            if (_this.$isDirty) {
                // If our model DID change while the save was in-flight,
                // update the pristine version of the model with what came back from the save,
                // but don't load the data into the `$data` prop.
                // This helps `$isDirty` to work as expected.
                _this._pristineDto = mapToDto(_this.$save.result);
            }
            else {
                // Only load the save response if the data hasn't changed since we sent it.
                // If the data has changed, loading the response would overwrite users' changes.
                updateFromModel(_this.$data, _this.$save.result);
                // Set the new state of our data as being clean (since we just made a change to it)
                _this.$isDirty = false;
            }
        });
        /**
         * A function for invoking the /delete endpoint, and a set of properties about the state of the last call.
         */
        this.$delete = this.$apiClient.$makeCaller("item", function (c) { return function () { return c.delete(_this.$primaryKey); }; });
        // Internal autosave state
        this._autoSaveState = new AutoCallState();
        if (initialData) {
            if (!initialData.$metadata) {
                throw "Initial data must have a $metadata property.";
            }
            else if (initialData.$metadata != $metadata) {
                throw "Initial data must have a $metadata value for type " + $metadata.name + ".";
            }
            else {
                this.$data = initialData;
            }
        }
        else {
            this.$data = convertToModel({}, $metadata);
        }
        this.$isDirty = false;
    }
    Object.defineProperty(ViewModel.prototype, "$primaryKey", {
        /**
         * Gets or sets the primary key of the ViewModel's data.
         */
        get: function () { return this.$data[this.$metadata.keyProp.name]; },
        set: function (val) { this.$data[this.$metadata.keyProp.name] = val; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ViewModel.prototype, "$isDirty", {
        /**
         * Returns true if the values of the savable data properties of this ViewModel
         * have changed since the last load, save, or the last time $isDirty was set to false.
         */
        get: function () { return JSON.stringify(mapToDto(this.$data)) != JSON.stringify(this._pristineDto); },
        set: function (val) { if (val)
            throw "Can't set $isDirty to true manually"; this._pristineDto = mapToDto(this.$data); },
        enumerable: true,
        configurable: true
    });
    /**
     * Starts auto-saving of the instance's data properties when changes occur.
     * Only properties which will be sent in save requests are watched -
     * navigation properties are not considered.
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param wait Time in milliseconds to debounce saves for
     * @param predicate A function that will be called before saving that can return false to prevent a save.
     */
    ViewModel.prototype.$startAutoSave = function (vue, wait, predicate) {
        var _this = this;
        if (wait === void 0) { wait = 1000; }
        this.$stopAutoSave();
        var enqueueSave = debounce(function () {
            if (!_this._autoSaveState.on)
                return;
            if (_this.$save.isLoading) {
                // Save already in progress. Enqueue another attempt.
                enqueueSave();
            }
            else if (_this.$isDirty) {
                // Check if we should save.
                if (predicate && !predicate(_this)) {
                    // If not, try again after the timer.
                    enqueueSave();
                    return;
                }
                // No saves in progress - go ahead and save now.
                _this.$save()
                    // After the save finishes, attempt another autosave.
                    // If the model has become dirty since the last save,
                    // we need to save again.
                    // This will happen if the state of the model changes while the save
                    // is in-flight.
                    .then(enqueueSave);
            }
        }, wait);
        var watcher = vue.$watch(function () { return _this.$isDirty; }, enqueueSave);
        startAutoCall(this._autoSaveState, vue, watcher, enqueueSave);
    };
    /** Stops auto-saving if it is currently enabled. */
    ViewModel.prototype.$stopAutoSave = function () {
        stopAutoCall(this._autoSaveState);
    };
    /**
     * Returns a string representation of the object, or one of its properties, suitable for display.
     * @param prop If provided, specifies a property whose value will be displayed.
     * If omitted, the whole object will be represented.
     */
    ViewModel.prototype.$display = function (prop) {
        if (!prop)
            return modelDisplay(this);
        return propDisplay(this, prop);
    };
    /**
     * Creates a new instance of an item for the specified child collection, adds it to that collection, and returns the item.
     * For class collections, this will be a valid implementation of the corresponding model interface.
     * For non-class collections, this will be null.
     * @param prop The name of the collection property, or the metadata representing it.
     */
    ViewModel.prototype.$addChild = function (prop) {
        var propMeta = resolvePropMeta(this.$metadata, prop);
        var collection = this.$data[propMeta.name];
        if (!Array.isArray(collection)) {
            collection = this.$data[propMeta.name] = [];
        }
        if (propMeta.role == "collectionNavigation") {
            var newModel = convertToModel({}, propMeta.itemType.typeDef);
            var foreignKey = propMeta.foreignKey;
            if (foreignKey) {
                newModel[foreignKey.name] = this.$primaryKey;
            }
            collection.push(newModel);
            return newModel;
        }
        else {
            // TODO: handle non-navigation collections (value collections of models/objects)
            collection.push(null);
            return null;
        }
    };
    return ViewModel;
}());
export { ViewModel };
// Model<TModel["$metadata"]>
var ListViewModel = /** @class */ (function () {
    function ListViewModel(
    // The following MUST be declared in the constructor so its value will be available to property initializers.
    /** The metadata representing the type of data that this ViewModel handles. */
    $metadata, 
    /** Instance of an API client for the model through which direct, stateless API requests may be made. */
    $apiClient) {
        var _this = this;
        this.$metadata = $metadata;
        this.$apiClient = $apiClient;
        this.$params = new ListParameters();
        /**
         * A function for invoking the /load endpoint, and a set of properties about the state of the last call.
         */
        this.$load = this.$apiClient
            .$makeCaller("list", function (c) { return function () { return c.list(_this.$params); }; });
        /**
         * A function for invoking the /count endpoint, and a set of properties about the state of the last call.
         */
        this.$count = this.$apiClient
            .$makeCaller("item", function (c) { return function () { return c.count(_this.$params); }; });
        // Internal autoload state
        this._autoLoadState = new AutoCallState();
    }
    Object.defineProperty(ListViewModel.prototype, "$items", {
        // TODO: merge in the result, don't replace the existing one??
        // .onFulfilled(() => { this.$data = this.$load.result || this.$data; this.$isDirty = false; })
        /**
         * The current set of items that have been loaded into this ListViewModel.
         */
        get: function () { return this.$load.result; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ListViewModel.prototype, "$hasPreviousPage", {
        /** True if the page set in $params.page is greater than 1 */
        get: function () { return (this.$params.page || 1) > 1; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(ListViewModel.prototype, "$hasNextPage", {
        /** True if the count retrieved from the last load indicates that there are pages after the page set in $params.page */
        get: function () { return (this.$params.page || 1) < (this.$load.pageCount || 0); },
        enumerable: true,
        configurable: true
    });
    /** Decrement the page parameter by 1 if there is a previous page. */
    ListViewModel.prototype.$previousPagePage = function () { if (this.$hasPreviousPage)
        this.$params.page = (this.$params.page || 1) - 1; };
    /** Increment the page parameter by 1 if there is a next page. */
    ListViewModel.prototype.$nextPage = function () { if (this.$hasNextPage)
        this.$params.page = (this.$params.page || 1) + 1; };
    /**
     * Starts auto-loading of the list as changes to its parameters occur.
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param wait Time in milliseconds to debounce loads for
     * @param predicate A function that will be called before loading that can return false to prevent a load.
     */
    ListViewModel.prototype.$startAutoLoad = function (vue, wait, predicate) {
        var _this = this;
        if (wait === void 0) { wait = 1000; }
        this.$stopAutoLoad();
        var enqueueLoad = debounce(function () {
            if (!_this._autoLoadState.on)
                return;
            // Check the predicate again incase its state has changed while we were waiting for the debouncing timer.
            if (predicate && !predicate(_this)) {
                return;
            }
            if (_this.$load.isLoading && _this.$load.concurrencyMode != "cancel") {
                // Load already in progress. Enqueue another attempt.
                enqueueLoad();
            }
            else {
                // No loads in progress, or concurrency is set to cancel - go for it.
                _this.$load();
            }
        }, wait);
        var onChange = function () {
            if (predicate && !predicate(_this)) {
                return;
            }
            enqueueLoad();
        };
        var watcher = vue.$watch(function () { return _this.$params; }, onChange, { deep: true });
        startAutoCall(this._autoLoadState, vue, watcher, enqueueLoad);
    };
    /** Stops auto-loading if it is currently enabled. */
    ListViewModel.prototype.$stopAutoLoad = function () {
        stopAutoCall(this._autoLoadState);
    };
    return ListViewModel;
}());
export { ListViewModel };
/* Internal members/helpers */
var AutoCallState = /** @class */ (function () {
    function AutoCallState() {
        this.on = false;
        this.cleanup = null;
        // Seal to prevent unnessecary reactivity
        return Object.seal(this);
    }
    return AutoCallState;
}());
function startAutoCall(state, vue, watcher, debouncer) {
    var destroyHook = function () { return stopAutoCall(state); };
    vue.$on('hook:beforeDestroy', destroyHook);
    state.cleanup = function () {
        if (!state.on)
            return;
        // Destroy the watcher
        watcher();
        // Cancel the debouncing timer if there is one.
        if (debouncer)
            debouncer.cancel();
        // Cleanup the hook, in case we're not responding to beforeDestroy but instead to a direct call to stopAutoCall.
        // If we didn't do this, autosave could later get disabled when the original component is destroyed, 
        // even though if was later attached to a different component that is still alive.
        vue.$off('hook:beforeDestroy', destroyHook);
    };
    state.on = true;
}
function stopAutoCall(state) {
    if (!state.on)
        return;
    state.cleanup();
    state.on = false;
}

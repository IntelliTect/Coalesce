"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var debounce_1 = require("lodash-es/debounce");
var metadata_1 = require("./metadata");
var model_1 = require("./model");
/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
function defineProps(ctor, metadata) {
    Object.defineProperties(ctor.prototype, Object.keys(metadata.props).reduce(function (descriptors, propName) {
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
exports.defineProps = defineProps;
/*
DESIGN NOTES
    - ViewModel deliberately has TModel as its only type parameter.
        The type of the metadata is always accessed off of TModel as TModel["$metadata"].
        This makes the intellisense in IDEs quite nice. If TMeta is a type param,
        we end up with the type of implemented classes taking several pages of the intellisense tooltip.
        With this, we can still strongly type off of known information of TMeta (like PropNames<TModel["$metadata"]>),
        but without it cluttering up tooltips with basically the entire type structure of the metadata.
    - ViewModels never instantiate other ViewModels on the users' behalf. ViewModels must always be instantiated explicitly.
        This makes it much easier to reason about the behavior of a program
        when Coalesce isn't creating ViewModel instances on the developers' behalf.
        It prevents the existance of deeply nested, difficult-to-access (or even find at all) instances
        that are difficult to configure. Ideally, all ViewModels exist on instances of components.
*/
var ViewModel = /** @class */ (function () {
    // protected _lateInitialize<T>(initializer: (this: this) => T) {
    // }
    function ViewModel(
    // The following MUST be declared in the constructor so they will be available to property initializers.
    $metadata, 
    /**
     * Instance of an API client for the model through which direct, stateless API requests may be made.
     */
    $apiClient, initialData) {
        var _this = this;
        this.$metadata = $metadata;
        this.$apiClient = $apiClient;
        /**
         * A function for invoking the /get endpoint, and a set of properties about the state of the last call.
         */
        this.$load = this.$apiClient.$makeCaller("item", function (c) { return function (id) { return c.get(id != null ? id : _this.$primaryKey); }; })
            .onFulfilled(function () { _this.$data = _this.$load.result || _this.$data; _this.$isDirty = false; });
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
            if (!_this.$isDirty) {
                // Only load the save response if the data hasn't changed since we sent it.
                // If the data has changed, loading the response would overwrite users' changes.
                // TODO: merge in the result, don't replace the existing one.
                _this.$data = _this.$save.result || _this.$data;
                // Set the new state of our data as being clean (since we just made a change to it)
                _this.$isDirty = false;
            }
        });
        /**
         * A function for invoking the /delete endpoint, and a set of properties about the state of the last call.
         */
        this.$delete = this.$apiClient.$makeCaller("item", function (c) { return function () { return c.delete(_this.$primaryKey); }; });
        // Internal autosave state - seal to prevent unnessecary reactivity
        this._autoSaveState = Object.seal({ on: false, cleanup: null });
        this.$metadata = $metadata;
        // Define proxy getters/setters to the underlying $data object.
        // Object.defineProperties(this, Object.keys($metadata.props).reduce((descriptors, propName) => {
        //     descriptors[propName] = {
        //         enumerable: true,
        //         get: function(this: self) {
        //             return this.$data[propName]
        //         },
        //         set: function(this: self, val: any) {
        //             this.$data[propName] = val
        //         }
        //     }
        //     return descriptors
        // }, {} as PropertyDescriptorMap))
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
            this.$data = model_1.convertToModel({}, $metadata);
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
        get: function () { return JSON.stringify(model_1.mapToDto(this.$data)) != JSON.stringify(this._pristineDto); },
        set: function (val) { if (val)
            throw "Can't set $isDirty to true manually"; this._pristineDto = model_1.mapToDto(this.$data); },
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
        var enqueueSave = debounce_1.default(function () {
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
                _this.$save();
            }
        }, wait);
        var watcher = vue.$watch(function () { return _this.$isDirty; }, enqueueSave);
        var destroyHook = function () { return _this.$stopAutoSave(); };
        vue.$on('hook:beforeDestroy', destroyHook);
        this._autoSaveState.cleanup = function () {
            if (!_this._autoSaveState.on)
                return;
            watcher(); // This destroys the watcher
            enqueueSave.cancel();
            // Cleanup the hook, in case we're not responding to beforeDestroy but instead to a direct call to $stopAutoSave.
            // If we didn't do this, autosave could later get disabled when the original component is destroyed, 
            // even though if was later attached to a different component that is still alive.
            vue.$off('hook:beforeDestroy', destroyHook);
        };
        this._autoSaveState.on = true;
    };
    /** Stops auto-saving if it is currently enabled. */
    ViewModel.prototype.$stopAutoSave = function () {
        if (!this._autoSaveState.on)
            return;
        this._autoSaveState.cleanup();
        this._autoSaveState.on = false;
    };
    /**
     * Returns a string representation of the object, or one of its properties, suitable for display.
     * @param prop If provided, specifies a property whose value will be displayed.
     * If omitted, the whole object will be represented.
     */
    ViewModel.prototype.$display = function (prop) {
        if (!prop)
            return model_1.modelDisplay(this);
        return model_1.propDisplay(this, prop);
    };
    /**
     * Creates a new instance of an item for the specified child collection, adds it to that collection, and returns the item.
     * For class collections, this will be a valid implementation of the corresponding model interface.
     * For non-class collections, this will be null.
     * @param prop The name of the collection property, or the metadata representing it.
     */
    ViewModel.prototype.$addChild = function (prop) {
        var propMeta = metadata_1.resolvePropMeta(this.$metadata, prop);
        var collection = this.$data[propMeta.name];
        if (!Array.isArray(collection)) {
            collection = this.$data[propMeta.name] = [];
        }
        var typeDef = propMeta.typeDef;
        if (metadata_1.isClassType(typeDef)) {
            var newModel = model_1.convertToModel({}, typeDef);
            var foreignKey = propMeta.foreignKey;
            if (foreignKey) {
                newModel[foreignKey.name] = this.$primaryKey;
            }
            collection.push(newModel);
            return newModel;
        }
        else {
            collection.push(null);
            return null;
        }
    };
    return ViewModel;
}());
exports.ViewModel = ViewModel;

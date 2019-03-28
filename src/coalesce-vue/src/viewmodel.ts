
import Vue from 'vue';

import { ModelType, CollectionProperty, PropertyOrName, resolvePropMeta, PropNames } from './metadata';
import { ModelApiClient, ListParameters, DataSourceParameters } from './api-client';
import { Model, modelDisplay, propDisplay, mapToDto, convertToModel, updateFromModel } from './model';
import { Indexable } from './util';
import { debounce } from 'lodash-es'
import { Cancelable } from 'lodash'

// These imports allow TypeScript to correctly name types in the generated declarations.
// Without them, it will generate some horrible, huge relative paths that won't work on any other machine.
// For example: import("../../../../Coalesce/src/coalesce-vue/src/api-client").ItemResult<TModel>
import * as apiClient from './api-client';
import * as axios from 'axios';


/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
export function defineProps<T extends new() => ViewModel<any, any>>(ctor: T, metadata: ModelType)
{
    Object.defineProperties(ctor.prototype,     
    Object
        .keys(metadata.props)
        .reduce(function (descriptors, propName) {
            descriptors[propName] = {
                enumerable: true,
                get: function(this: InstanceType<T>) {
                    return this.$data[propName]
                },
                set: function(this: InstanceType<T>, val: any) {
                    this.$data[propName] = val
                }
            }
            return descriptors
        }, {} as PropertyDescriptorMap)
    )
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

export abstract class ViewModel<
    TModel extends Model<ModelType>,
    TApi extends ModelApiClient<TModel>,
> implements Model<TModel["$metadata"]> {
    /**
     * Object which holds all of the data represented by this ViewModel.
     */
    // Will always be reactive - is definitely assigned in the ctor.
    public $data: Indexable<TModel>

    // Must be initialized so it will be reactive.
    // If this isn't reactive, $isDirty won't be reactive.
	// Technically this will always be initialized by the setting of `$isDirty` in the ctor.
    private _pristineDto: any = null;

    /**
     * Gets or sets the primary key of the ViewModel's data.
     */
    public get $primaryKey() { return this.$data[this.$metadata.keyProp.name] }
    public set $primaryKey(val) { this.$data[this.$metadata.keyProp.name] = val }

    /**
     * Returns true if the values of the savable data properties of this ViewModel 
     * have changed since the last load, save, or the last time $isDirty was set to false.
     */
    public get $isDirty() { return JSON.stringify(mapToDto(this.$data)) != JSON.stringify(this._pristineDto)}
    public set $isDirty(val) { if (val) throw "Can't set $isDirty to true manually"; this._pristineDto = mapToDto(this.$data) }


    /** The parameters that will be passed to `/get`, `/save`, and `/delete` calls. */
    public $params = new DataSourceParameters();

    /** Wrapper for `$params.dataSource` */
    public get $dataSource() { return this.$params.dataSource }
    public set $dataSource(val) { this.$params.dataSource = val } 

    /** Wrapper for `$params.includes` */
    public get $includes() { return this.$params.includes }
    public set $includes(val) { this.$params.includes = val } 

    /**
     * A function for invoking the `/get` endpoint, and a set of properties about the state of the last call.
     */
    public $load = this.$apiClient.$makeCaller("item",
        (c, id?: string | number) => c.get(id != null ? id : this.$primaryKey, this.$params))
        .onFulfilled(() => { 
            if (this.$load.result) {
                updateFromModel(this.$data, this.$load.result);
                this.$isDirty = false; 
            }

        })

    /** Whether or not to reload the ViewModel's `$data` with the response received from the server after a call to .$save(). */
    public $loadResponseFromSaves = true;

    /**
     * A function for invoking the `/save` endpoint, and a set of properties about the state of the last call.
     */
    public $save = this.$apiClient.$makeCaller("item", 
        c => {
            // Before we make the save call, set isDirty = false.
            // This lets us detect changes that happen to the model while our save request is pending.
            // If the model is dirty when the request completes, we'll not load the response from the server.
            this.$isDirty = false;
            return c.save(this.$data, this.$params);
        })
        .onFulfilled(() => { 
            if (!this.$save.result) {
                // Can't do anything useful if the save returned no data.
                return;
            }

            if (this.$isDirty) {
                // If our model DID change while the save was in-flight,
                // update the pristine version of the model with what came back from the save,
                // and load the primary key, but don't load the data into the `$data` prop.
                // This helps `$isDirty` to work as expected.
                this._pristineDto = mapToDto(this.$save.result)

                // The PK *MUST* be loaded so that the PK returned by a creation save call
                // will be used by subsequent update calls.
                this.$primaryKey = (this.$save.result as Indexable<TModel>)[this.$metadata.keyProp.name]
            } else {

                // else: Only load the save response if the data hasn't changed since we sent it.
                // If the data has changed, loading the response would overwrite users' changes.
                if (this.$loadResponseFromSaves) {
                    updateFromModel(this.$data, this.$save.result);
                } else {
                    // The PK *MUST* be loaded so that the PK returned by a creation save call
                    // will be used by subsequent update calls.
                    this.$primaryKey = (this.$save.result as Indexable<TModel>)[this.$metadata.keyProp.name]
                }

                // Set the new state of our data as being clean (since we just made a change to it)
                this.$isDirty = false;
            }
        })
        
    /**
     * A function for invoking the `/delete` endpoint, and a set of properties about the state of the last call.
     */
    public $delete = this.$apiClient.$makeCaller("item",
        c => c.delete(this.$primaryKey, this.$params))

    // Internal autosave state
    private _autoSaveState = new AutoCallState()

    /**
     * Starts auto-saving of the instance's data properties when changes occur. 
     * Only properties which will be sent in save requests are watched - 
     * navigation properties are not considered.
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param wait Time in milliseconds to debounce saves for
     * @param predicate A function that will be called before saving that can return false to prevent a save.
     */
    public $startAutoSave(vue: Vue, wait: number = 1000, predicate?: (viewModel: this) => boolean) {
        this.$stopAutoSave()

        const enqueueSave = debounce(() => {
            if (!this._autoSaveState.on) return;
            if (this.$save.isLoading) {
                // Save already in progress. Enqueue another attempt.
                enqueueSave()
            } else if (this.$isDirty) {

                // Check if we should save.
                if (predicate && !predicate(this)) {
                    // If not, try again after the timer.
                    enqueueSave()
                    return 
                }

                // No saves in progress - go ahead and save now.
                this.$save()
                    // After the save finishes, attempt another autosave.
                    // If the model has become dirty since the last save,
                    // we need to save again.
                    // This will happen if the state of the model changes while the save
                    // is in-flight.
                    .then(enqueueSave) 
            }
        }, wait)

        const watcher = vue.$watch(() => this.$isDirty, enqueueSave);
        startAutoCall(this._autoSaveState, vue, watcher, enqueueSave);
    }

    /** Stops auto-saving if it is currently enabled. */
    public $stopAutoSave() {
        stopAutoCall(this._autoSaveState);
    }

    /**
     * Returns a string representation of the object, or one of its properties, suitable for display.
     * @param prop If provided, specifies a property whose value will be displayed. 
     * If omitted, the whole object will be represented.
     */
    public $display(prop?: PropertyOrName<TModel["$metadata"]>) {
        if (!prop) return modelDisplay(this);
        return propDisplay(this, prop);
    }

    /**
     * Creates a new instance of an item for the specified child collection, adds it to that collection, and returns the item.
     * For class collections, this will be a valid implementation of the corresponding model interface.
     * For non-class collections, this will be null.
     * @param prop The name of the collection property, or the metadata representing it.
     */
    public $addChild(prop: CollectionProperty | PropNames<TModel["$metadata"], CollectionProperty>) {
        const propMeta = resolvePropMeta<CollectionProperty>(this.$metadata, prop)
        var collection: Array<any> = this.$data[propMeta.name];

        if (!Array.isArray(collection)) {
            collection = this.$data[propMeta.name] = [];
        }

        if (propMeta.role == "collectionNavigation") {
            var newModel = convertToModel({}, propMeta.itemType.typeDef);
            const foreignKey = propMeta.foreignKey
            if (foreignKey){
                (newModel as Indexable<TModel>)[foreignKey.name] = this.$primaryKey
            }
            collection.push(newModel);
            return newModel;
        } else {
            // TODO: handle non-navigation collections (value collections of models/objects)
            collection.push(null);
            return null;
        }
    }

    constructor(
        // The following MUST be declared in the constructor so its value will be available to property initializers.

        /** The metadata representing the type of data that this ViewModel handles. */
        public readonly $metadata: TModel["$metadata"], 

        /** Instance of an API client for the model through which direct, stateless API requests may be made. */
        public readonly $apiClient: TApi,

        initialData?: TModel
    ) {

        if (initialData) {
            if (!initialData.$metadata) {
                throw `Initial data must have a $metadata property.`
            } else if (initialData.$metadata != $metadata) {
                throw `Initial data must have a $metadata value for type ${$metadata.name}.`
            } else {
                this.$data = initialData
            }
        } else {
            this.$data = convertToModel({}, $metadata);
        }

        this.$isDirty = false;
    }
}

// Model<TModel["$metadata"]>
export abstract class ListViewModel<
    TModel extends Model<ModelType>,
    TApi extends ModelApiClient<TModel>,
> {

    /** The parameters that will be passed to `/list` and `/count` calls. */
    public $params = new ListParameters();

    /**
     * The current set of items that have been loaded into this ListViewModel.
     */
    public get $items() { return this.$load.result }

    /** True if the page set in $params.page is greater than 1 */
    public get $hasPreviousPage() { return (this.$params.page || 1) > 1 }
    /** True if the count retrieved from the last load indicates that there are pages after the page set in $params.page */
    public get $hasNextPage() { return (this.$params.page || 1) < (this.$load.pageCount || 0) }

    /** Decrement the page parameter by 1 if there is a previous page. */
    public $previousPagePage() { if (this.$hasPreviousPage) this.$params.page = (this.$params.page || 1) - 1 }
    /** Increment the page parameter by 1 if there is a next page. */
    public $nextPage() { if (this.$hasNextPage) this.$params.page = (this.$params.page || 1) + 1 }


    /**
     * A function for invoking the `/load` endpoint, and a set of properties about the state of the last call.
     */
    public $load = this.$apiClient
        .$makeCaller("list", c => c.list(this.$params));
        // TODO: merge in the result, don't replace the existing one??
       // .onFulfilled(() => { this.$data = this.$load.result || this.$data; this.$isDirty = false; })
    
    /**
     * A function for invoking the `/count` endpoint, and a set of properties about the state of the last call.
     */
    public $count = this.$apiClient
        .$makeCaller("item", c => c.count(this.$params));

    // Internal autoload state
    private _autoLoadState = new AutoCallState()

    /**
     * Starts auto-loading of the list as changes to its parameters occur. 
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param wait Time in milliseconds to debounce loads for
     * @param predicate A function that will be called before loading that can return false to prevent a load.
     */
    public $startAutoLoad(vue: Vue, wait: number = 1000, predicate?: (viewModel: this) => boolean) {
        this.$stopAutoLoad()

        const enqueueLoad = debounce(() => {
            if (!this._autoLoadState.on) return;

            // Check the predicate again incase its state has changed while we were waiting for the debouncing timer.
            if (predicate && !predicate(this)) { return  }

            if (this.$load.isLoading && this.$load.concurrencyMode != "cancel") {
                // Load already in progress. Enqueue another attempt.
                enqueueLoad()
            } else {
                // No loads in progress, or concurrency is set to cancel - go for it.
                this.$load()
            }
        }, wait)
        
        const onChange = () => {
            if (predicate && !predicate(this)) { return }
            enqueueLoad();
        }

        const watcher = vue.$watch(() => this.$params, onChange, { deep: true });
        startAutoCall(this._autoLoadState, vue, watcher, enqueueLoad);
    }

    /** Stops auto-loading if it is currently enabled. */
    public $stopAutoLoad() {
        stopAutoCall(this._autoLoadState);
    }


    constructor(
        // The following MUST be declared in the constructor so its value will be available to property initializers.

        /** The metadata representing the type of data that this ViewModel handles. */
        public readonly $metadata: TModel["$metadata"], 

        /** Instance of an API client for the model through which direct, stateless API requests may be made. */
        public readonly $apiClient: TApi
    ) {
    }
}




/* Internal members/helpers */

class AutoCallState {
    on: boolean = false
    cleanup: Function | null = null

    constructor() {
        // Seal to prevent unnessecary reactivity
        return Object.seal(this);
    }
}

function startAutoCall(state: AutoCallState, vue: Vue, watcher: () => void, debouncer?: Cancelable) {
    const destroyHook = () => stopAutoCall(state)

    vue.$on('hook:beforeDestroy', destroyHook)
    state.cleanup = () => {
        if (!state.on) return;
        // Destroy the watcher
        watcher()
        // Cancel the debouncing timer if there is one.
        if (debouncer) debouncer.cancel()
        // Cleanup the hook, in case we're not responding to beforeDestroy but instead to a direct call to stopAutoCall.
        // If we didn't do this, autosave could later get disabled when the original component is destroyed, 
        // even though if was later attached to a different component that is still alive.
        vue.$off('hook:beforeDestroy', destroyHook)
    }
    state.on = true;
}

function stopAutoCall(state: AutoCallState) {
    if (!state.on) return;
    state.cleanup!()
    state.on = false
}
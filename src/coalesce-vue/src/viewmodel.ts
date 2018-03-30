
import Vue from 'vue';
import { AxiosResponse, AxiosError } from 'axios';
import debounce from 'lodash-es/debounce';

import { ModelType, CollectionProperty, PropertyOrName, resolvePropMeta, PropNames, ObjectType } from './metadata';
import { ApiClient, ItemResult, ItemApiState } from './api-client';
import { Model, modelDisplay, propDisplay, mapToDto, convertToModel } from './model';
import { Indexable } from './util';

/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
export function defineProps<T extends new() => ViewModel<any, any>>(ctor: T, metadata: ModelType)
{
  Object.defineProperties(ctor.prototype,     
    Object.keys(metadata.props).reduce(function (descriptors, propName) {
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
  }, {} as PropertyDescriptorMap))
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
*/

export abstract class ViewModel<
    TModel extends Model<ModelType>,
    TApi extends ApiClient<TModel>,
> implements Model<TModel["$metadata"]> {
    /**
     * Object which holds all of the data represented by this ViewModel.
     */
    // Will always be reactive - is definitely assigned in the ctor.
    public $data: Indexable<TModel>

    // Won't be reactive, and shouldn't be.
    private _pristineDto: any;

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

    /**
     * A function for invoking the /get endpoint, and a set of properties about the state of the last call.
     */
    public $load = this.$apiClient.$makeCaller("item",
        c => (id?: string | number) => c.get(id != null ? id : this.$primaryKey))
        // TODO: merge in the result, don't replace the existing one.
        .onFulfilled(() => { this.$data = this.$load.result || this.$data; this.$isDirty = false; })

    /**
     * A function for invoking the /save endpoint, and a set of properties about the state of the last call.
     */
    public $save = this.$apiClient.$makeCaller("item", 
        c => () => {
            // Before we make the save call, set isDirty = false.
            // This lets us detect changes that happen to the model while our save request is pending.
            // If the model is dirty when the request completes, we'll not load the response from the server.
            this.$isDirty = false;
            return c.save(this.$data);
        })
        .onFulfilled(() => { 
            if (!this.$isDirty){
                // Only load the save response if the data hasn't changed since we sent it.
                // If the data has changed, loading the response would overwrite users' changes.
                // TODO: merge in the result, don't replace the existing one.
                this.$data = this.$save.result || this.$data; 

                // Set the new state of our data as being clean (since we just made a change to it)
                this.$isDirty = false;
            }
        })
        
    /**
     * A function for invoking the /delete endpoint, and a set of properties about the state of the last call.
     */
    public $delete = this.$apiClient.$makeCaller("item",
        c => () => c.delete(this.$primaryKey))

    // Internal autosave state - seal to prevent unnessecary reactivity
    private _autoSaveState = Object.seal<{
        on: boolean,
        cleanup: Function | null
    }>({ on: false, cleanup: null})

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
            }
        }, wait)

        const watcher = vue.$watch(() => this.$isDirty, enqueueSave);

        const destroyHook = () => this.$stopAutoSave()
        vue.$on('hook:beforeDestroy', destroyHook)
        this._autoSaveState.cleanup = () => {
            if (!this._autoSaveState.on) return;
            watcher() // This destroys the watcher
            enqueueSave.cancel()
            // Cleanup the hook, in case we're not responding to beforeDestroy but instead to a direct call to $stopAutoSave.
            // If we didn't do this, autosave could later get disabled when the original component is destroyed, 
            // even though if was later attached to a different component that is still alive.
            vue.$off('hook:beforeDestroy', destroyHook)
        }
        this._autoSaveState.on = true;
    }

    /** Stops auto-saving if it is currently enabled. */
    public $stopAutoSave() {
        if (!this._autoSaveState.on) return;
        this._autoSaveState.cleanup!()
        this._autoSaveState.on = false
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


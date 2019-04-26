
import Vue from 'vue';

import { ModelType, CollectionProperty, PropertyOrName, resolvePropMeta, PropNames, Method, ClassType } from './metadata';
import { ModelApiClient, ListParameters, DataSourceParameters, ParamsObject, ListApiState, ItemApiState, ItemResultPromise, ListResultPromise } from './api-client';
import { Model, modelDisplay, propDisplay, mapToDto, convertToModel, updateFromModel } from './model';
import { Indexable } from './util';
import { debounce } from 'lodash-es'
import { Cancelable } from 'lodash'

// These imports allow TypeScript to correctly name types in the generated declarations.
// Without them, it will generate some horrible, huge relative paths that won't work on any other machine.
// For example: import("../../../../Coalesce/src/coalesce-vue/src/api-client").ItemResult<TModel>
import * as apiClient from './api-client';
import * as axios from 'axios';


// /**
//  * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
//  * @param ctor The class to add wrapper properties to
//  * @param metadata The metadata describing the properties to add.
//  */
// export function defineProps<T extends new() => ViewModel<any, any>>(ctor: T, metadata: ModelType)
// {
//     Object.defineProperties(ctor.prototype,     
//     Object
//         .keys(metadata.props)
//         .reduce(function (descriptors, propName) {
//             descriptors[propName] = {
//                 enumerable: true,
//                 get: function(this: InstanceType<T>) {
//                     return this.$data[propName]
//                 },
//                 set: function(this: InstanceType<T>, val: any) {
//                     this.$data[propName] = val
//                 }
//             }
//             return descriptors
//         }, {} as PropertyDescriptorMap)
//     )
// }

/*
DESIGN NOTES
    - ViewModel deliberately does not have TModel as a type parameter.
        The type of the metadata is always accessed off of TModel as TModel["$metadata"].
        This makes the intellisense in IDEs quite nice. If TMeta is a type param,
        we end up with the type of implemented classes taking several pages of the intellisense tooltip.
        With this, we can still strongly type off of known information of TMeta (like PropNames<TModel["$metadata"]>),
        but without cluttering up tooltips with the entire type structure of the metadata.
*/

type AutoSaveOptions<TThis> = {
    /** Time, in milliseconds, to debounce saves for.  */
    wait?: number, 
    /** A function that will be called before autosaving that can return false to prevent a save. */
    predicate?: (viewModel: TThis) => boolean,
    /** If true, auto-saving will also be enabled for all view models that are 
     * reachable from the navigation properties & collections of the current view model.
     * 
     * If a predicate is provided, the predicate will only affect the current view model.
     * Explicitly call `$startAutoSave` on other ViewModels if they require predicates. */
    deep?: boolean
}
export abstract class ViewModel<
    TModel extends Model<ModelType> = Model<ModelType>,
    TApi extends ModelApiClient<TModel> = ModelApiClient<TModel>,
    TPrimaryKey extends string | number = string | number
> implements Model<TModel["$metadata"]> {
    /**
     * Object which holds all of the data represented by this ViewModel.
     */
    // Will always be reactive - is definitely assigned in the ctor.
    // public $data: Indexable<TModel>

    // Must be initialized so it will be reactive.
    // If this isn't reactive, $isDirty won't be reactive.
	// Technically this will always be initialized by the setting of `$isDirty` in the ctor.
    private _pristineDto: any = null;

    /**
     * Gets or sets the primary key of the ViewModel's data.
     */
    public get $primaryKey() { return (this as any as Indexable<TModel>)[this.$metadata.keyProp.name] as TPrimaryKey }
    public set $primaryKey(val) { (this as any as Indexable<TModel>)[this.$metadata.keyProp.name] = val }

    /**
     * Returns true if the values of the savable data properties of this ViewModel 
     * have changed since the last load, save, or the last time $isDirty was set to false.
     */
    public get $isDirty() { return JSON.stringify(mapToDto(this)) != JSON.stringify(this._pristineDto)}
    public set $isDirty(val) { if (val) throw "Can't set $isDirty to true manually"; this._pristineDto = mapToDto(this) }


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
        (c, id?: TPrimaryKey) => c.get(id != null ? id : this.$primaryKey, this.$params))
        .onFulfilled(() => { 
            if (this.$load.result) {
                this.$loadFromModel(this.$load.result);
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
            return c.save(this as any as TModel, this.$params);
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

                // else: model isn't dirty.
                // Since the data hasn't changed since we sent the save (since $isDirty === false),
                // se can safely load the save response, since we know we won't be overwriting any local changes.
                // If the data had changed, loading the response would overwrite user input and/or other changes to the data.
                if (this.$loadResponseFromSaves) {
                    this.$loadFromModel(this.$save.result);
                } else {
                    // The PK *MUST* be loaded so that the PK returned by a creation save call
                    // will be used by subsequent update calls.
                    this.$primaryKey = (this.$save.result as Indexable<TModel>)[this.$metadata.keyProp.name]
                }

                // Set the new state of our data as being clean (since we just made a change to it)
                this.$isDirty = false;
            }
        })

    public $loadFromModel(source: TModel) {
        updateFromModel(
            /* 
                All concrete viewmodels are valid interface implementations of their TModel.
                However, the base ViewModel class cannot explicitly declare that it implements TModel
                since TModel has no statically known members in this generic context. 
                So, we must soothe typescript.
            */
            this as unknown as TModel,
            source
        );
    }
        
    /**
     * A function for invoking the `/delete` endpoint, and a set of properties about the state of the last call.
     */
    public $delete = this.$apiClient.$makeCaller("item",
        c => c.delete(this.$primaryKey, this.$params))

    // Internal autosave state
    private _autoSaveState = new AutoCallState()

    /**
     * Starts auto-saving of the instance when changes to its savable data properties occur. 
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param options Options to control how the auto-saving is performed.
     */
    public $startAutoSave(
        vue: Vue, 
        options: AutoSaveOptions<this>
    ) {
        const { wait = 1000, predicate, deep } = options;
        this.$stopAutoSave()

        if (deep) {
            throw "'deep' option for autosaves is not yet implemented."
        }

        const enqueueSave = debounce(() => {
            if (!this._autoSaveState.active) return;
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
     * Creates a new instance of an item for the specified child model collection, 
     * adds it to that collection, and returns the item.
     * @param prop The name of the collection property, or the metadata representing it.
     */
    public $addChild<TChild extends ViewModel>(
        prop: CollectionProperty | PropNames<TModel["$metadata"], CollectionProperty>, 
        childFactory: (type: ModelType) => TChild
    ) {
        const propMeta = resolvePropMeta<CollectionProperty>(this.$metadata, prop)
        var collection: Array<any> | undefined = (this as any as Indexable<TModel>)[propMeta.name];

        if (!Array.isArray(collection)) {
            collection = (this as any as Indexable<TModel>)[propMeta.name] = [];
        }

        var newModel: TChild;
        if (propMeta.role == "collectionNavigation") {
            newModel = childFactory(propMeta.itemType.typeDef);
            const foreignKey = propMeta.foreignKey
            if (foreignKey) {
                (newModel as Indexable<TChild>)[foreignKey.name] = this.$primaryKey
            }
        } else if (propMeta.itemType.type == "model") {
            newModel = childFactory(propMeta.itemType.typeDef);
        }
        else {
            throw "$addChild only adds to collections of model properties."
        }

        // TODO: Set $parent, $parentCollection on `newModel`

        collection.push(newModel);
        return newModel;
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
                this.$loadFromModel(initialData);
            }
        }

        this.$isDirty = false;
    }
}

// Model<TModel["$metadata"]>
export abstract class ListViewModel<
    TModel extends Model<ModelType> = Model<ModelType>,
    TApi extends ModelApiClient<TModel> = ModelApiClient<TModel>,
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
    public $previousPagePage() { if (this.$hasPreviousPage) this.$params.page = (this.$params.page || 1) - 1; }
    /** Increment the page parameter by 1 if there is a next page. */
    public $nextPage() { if (this.$hasNextPage) this.$params.page = (this.$params.page || 1) + 1; }


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
            if (!this._autoLoadState.active) return;

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

// This ended up being replaced by the 'args' overloads of apiClient.$makeCaller.
// export class InstanceMethodViewModel<
//     TModel extends Model<ModelType>,
//     TApi extends ModelApiClient<Model<TModel["$metadata"]>>,
//     TMethod extends Method
// > {
    
//     $args: ParamsObject<TMethod>;

//     $invoke = this.$apiClient.$makeCaller(
//         this.$metadata as any, 
//         (c: TApi) => c.$invoke(this.$metadata as any, this.$args)) as any as (
//     TMethod["transportType"] extends "item" 
//         ? ItemApiState<[ParamsObject<TMethod>], any, any> & (() => ItemResultPromise<any>)
//         : ListApiState<[ParamsObject<TMethod>], any, any> & (() => ListResultPromise<any>)
//         )
        

//     constructor(public readonly $model: TModel, public readonly $metadata: TMethod, public readonly $apiClient: TApi) {
//         const self = this;
//         this.$args =  {
//             get id() { return ($model as any)[$model.$metadata.keyProp.name] },
//             ...Object
//                 .values(this.$metadata.params)
//                 .filter(p => p.name != 'id')
//                 .reduce((prev, cur) => {
//                     prev[cur.name] = null;
//                     return prev;
//                 }, {} as any)
//         }
//     }
// }

export interface ViewModelTypeLookup {
    [name: string]: {
        viewModel: new<T extends Model<ModelType>>(initialData?: T) => ViewModel<T>,
        listViewModel: new() => ListViewModel,
    }
}

export type ModelOf<T> = T extends ViewModel<infer TModel> ? TModel : never;

export type ViewModelFactory = (meta: ModelType) => ViewModel;



/**
 * Updates the target model with values from the source model.
 * @param target The viewmodel to be updated.
 * @param source The model whose values will be used to perform the update.
 */
export function updateFromModel<TMeta extends ModelType, TModel extends Model<TMeta>>(
    target: Model | ViewModel<TModel>, 
    source: TModel,
    viewModelFactory: ViewModelFactory
) {


    const metadata = target.$metadata;

    for (const prop of Object.values(metadata.props)) {
        const propName = prop.name;
        const sourceValue = (source as Indexable<TModel>)[propName];
        const currentValue = (target as Indexable<TModel>)[propName];

        switch (prop.role) {
            case "referenceNavigation":
            case "collectionNavigation":

        }

        switch (prop.type) {
            case "collection":
                switch (prop.role) {
                    case "value":
                        // something 
                        break;
                    case "collectionNavigation":
                        // something 
                        break;
                }
                break;
            case "model":
                if (currentValue == null && sourceValue) {
                    const newValue = viewModelFactory(prop.typeDef);
                    newValue.$loadFromModel(sourceValue);
                    (target as Indexable<typeof target>)[propName] = newValue;
                }
            case "object":
                if (currentValue == null && sourceValue) {
                    const newValue = viewModelFactory(prop.typeDef);
                    newValue.$loadFromModel(sourceValue);
                    (target as Indexable<typeof target>)[propName] = newValue;
                }
                break;
            default: 
                // @ts-ignore
                target[propName] = (source as Indexable<TModel>)[propName];
        }
    }
}


export class ViewModelMap {
    private map = new WeakMap<Model, ViewModel>();

    getOrCreate<T extends Model<ModelType>>(model: T, onCreate: (viewModel: ViewModel<T>) => void): ViewModel<T> {
        if (!this.map.has(model)) {
            const vm = this.factory(model)
            this.map.set(model, vm as ViewModel);
            if (onCreate) onCreate(vm);
            // Vue.util.defineReactive({vm}, 'vm', vm);
        };
        return this.map.get(model) as ViewModel<T>;
    }

    constructor(
        private factory: <T extends Model<ModelType>>(model: T) => ViewModel<T>
    ) {
        // Seal to prevent unnessecary reactivity
        return Object.seal(this);
    }
}


/* Internal members/helpers */

class AutoCallState {
    active: boolean = false
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
        if (!state.active) return;
        // Destroy the watcher
        watcher()
        // Cancel the debouncing timer if there is one.
        if (debouncer) debouncer.cancel()
        // Cleanup the hook, in case we're not responding to beforeDestroy but instead to a direct call to stopAutoCall.
        // If we didn't do this, autosave could later get disabled when the original component is destroyed, 
        // even though if was later attached to a different component that is still alive.
        vue.$off('hook:beforeDestroy', destroyHook)
    }
    state.active = true;
}

function stopAutoCall(state: AutoCallState) {
    if (!state.active) return;
    state.cleanup!()
    state.active = false
}
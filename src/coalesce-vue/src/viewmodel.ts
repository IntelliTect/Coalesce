
import Vue from 'vue';

import { ModelType, CollectionProperty, PropertyOrName, resolvePropMeta, PropNames, Method, ClassType, CollectionValue, ModelCollectionNavigationProperty } from './metadata';
import { ModelApiClient, ListParameters, DataSourceParameters, ParamsObject, ListApiState, ItemApiState, ItemResultPromise, ListResultPromise } from './api-client';
import { Model, modelDisplay, propDisplay, mapToDto, convertToModel, mapToModel } from './model';
import { Indexable } from './util';
import { debounce } from 'lodash-es'
import { Cancelable } from 'lodash'

// These imports allow TypeScript to correctly name types in the generated declarations.
// Without them, it will generate some horrible, huge relative paths that won't work on any other machine.
// For example: import("../../../../Coalesce/src/coalesce-vue/src/api-client").ItemResult<TModel>
import * as apiClient from './api-client';
import * as axios from 'axios';


/*
DESIGN NOTES
    - ViewModel deliberately does not have TModel as a type parameter.
        The type of the metadata is always accessed off of TModel as TModel["$metadata"].
        This makes the intellisense in IDEs quite nice. If TMeta is a type param,
        we end up with the type of implemented classes taking several pages of the intellisense tooltip.
        With this, we can still strongly type off of known information of TMeta (like PropNames<TModel["$metadata"]>),
        but without cluttering up tooltips with the entire type structure of the metadata.
*/

export abstract class ViewModel<
    TModel extends Model<ModelType> = any,
    TApi extends ModelApiClient<TModel> = any,
    TPrimaryKey extends string | number = any
> implements Model<TModel["$metadata"]> {
    /** Static lookup of all generated ViewModel types. */
    public static typeLookup: ViewModelTypeLookup | null = null;

    /**
     * Object which holds all of the data represented by this ViewModel.
     */
    // Will always be reactive - is definitely assigned in the ctor.
    // public $data: Indexable<TModel>

    // Must be initialized so it will be reactive.
    // If this isn't reactive, $isDirty won't be reactive.
	// Technically this will always be initialized by the setting of `$isDirty` in the ctor.
    private _pristineDto: any = null;

    protected $parent: any = null;
    protected $parentCollection: this[] | null = null;

    // Underlying object which will hold the backing values
    // of the custom getters/setters. Not for external use.
    // Must exist in order to for Vue to pick it up and add reactivity.
    private $data: TModel = convertToModel({}, this.$metadata);

    /**
     * Gets or sets the primary key of the ViewModel's data.
     */
    public get $primaryKey() { return (this as any as Indexable<TModel>)[this.$metadata.keyProp.name] as TPrimaryKey }
    public set $primaryKey(val) { (this as any as Indexable<TModel>)[this.$metadata.keyProp.name] = val }

    /**
     * Returns true if the values of the savable data properties of this ViewModel 
     * have changed since the last load, save, or the last time $isDirty was set to false.
     */
    public get $isDirty() { 
        return JSON.stringify(mapToDto(this)) != JSON.stringify(this._pristineDto)
    }
    public set $isDirty(val) { 
        if (val) throw "Can't set $isDirty to true manually"; 
        this._pristineDto = mapToDto(this) 
    }


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
        updateViewModelFromModel(
            /* 
                All concrete viewmodels are valid interface implementations of their TModel.
                However, the base ViewModel class cannot explicitly declare that it implements TModel
                since TModel has no statically known members in this generic context. 
                So, we must soothe typescript.
            */
            this as any,
            source
        );
        this.$isDirty = false;
    }
        
    /**
     * A function for invoking the `/delete` endpoint, and a set of properties about the state of the last call.
     */
    public $delete = this.$apiClient.$makeCaller("item",
        c => c.delete(this.$primaryKey, this.$params))
        .onFulfilled(() => {
            if (this.$parentCollection) {
                this.$parentCollection.splice(this.$parentCollection.indexOf(this), 1);
            }
        })

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

        const watcher = vue.$watch(() => {
            return this.$isDirty;
        }, enqueueSave);
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
    public $addChild(
        prop: ModelCollectionNavigationProperty | PropNames<TModel["$metadata"], ModelCollectionNavigationProperty>
    ) {
        const propMeta = resolvePropMeta<ModelCollectionNavigationProperty>(this.$metadata, prop)
        var collection: Array<any> | undefined = (this as any as Indexable<TModel>)[propMeta.name];

        if (!Array.isArray(collection)) {
            (this as any)[propMeta.name] = [];
            collection = (this as any)[propMeta.name] as any[];
        }

        if (propMeta.role == "collectionNavigation") {
            
            const newModel: any = mapToModel({}, propMeta.itemType.typeDef);
            const foreignKey = propMeta.foreignKey;
            (newModel)[foreignKey.name] = this.$primaryKey;
            
            // The ViewModelCollection will handle create a new ViewModel,
            // and setting $parent, $parentCollection.
            // TODO: Should it also handle setting of the foreign key?
            return collection[collection.push(newModel) - 1];
        }
        else {
            throw "$addChild only adds to collections of model properties."
        }
    }

    constructor(
        // The following MUST be declared in the constructor so its value will be available to property initializers.

        /** The metadata representing the type of data that this ViewModel handles. */
        public readonly $metadata: TModel["$metadata"], 

        /** Instance of an API client for the model through which direct, stateless API requests may be made. */
        public readonly $apiClient: TApi,

        initialData?: {} | TModel
    ) {
        if (initialData) {
            if (!('$metadata' in initialData)) {
                initialData = mapToModel(initialData, $metadata);
            } 
            else if (initialData.$metadata != $metadata) {
                throw `Initial data must have a $metadata value for type ${$metadata.name}.`
            } 

            this.$loadFromModel(initialData as TModel);
            // this.$isDirty will get set by $loadFromModel()
        } else {
            this.$isDirty = false;
        }

    }
}

// Model<TModel["$metadata"]>
export abstract class ListViewModel<
    TModel extends Model<ModelType> = Model<ModelType>,
    TApi extends ModelApiClient<TModel> = ModelApiClient<TModel>,
> {
    /** Static lookup of all generated ListViewModel types. */
    public static typeLookup: ListViewModelTypeLookup | null = null;

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
    public $previousPage() { if (this.$hasPreviousPage) this.$params.page = (this.$params.page || 1) - 1; }
    /** Increment the page parameter by 1 if there is a next page. */
    public $nextPage() { if (this.$hasNextPage) this.$params.page = (this.$params.page || 1) + 1; }

    public get $page() { return this.$params.page || 1 }
    public set $page(val) { this.$params.page = val }

    public get $pageSize() { return this.$params.pageSize || 1 }
    public set $pageSize(val) { this.$params.pageSize = val }

    public get $pageCount() { return this.$load.pageCount }

    /**
     * A function for invoking the `/load` endpoint, and a set of properties about the state of the last call.
     */
    public $load = this.$apiClient
        .$makeCaller("list", c => c.list(this.$params));
        // TODO: merge in the result, don't replace the existing one
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

export class ViewModelCollection<T extends ViewModel> extends Array<T> {
    
    private $metadata!: ModelCollectionNavigationProperty;
    private $parent!: ViewModel;

    push(...items: T[]): number {
        return Array.prototype.push.apply(this, items.map((val) => {
            if (val == null) {
                throw Error(`Cannot push null to a collection of ViewModels.`)
            }

            if (typeof val !== 'object') {
                throw Error(`Cannot assign a non-object to ${this.$metadata.name}`);
            }
            else if (!('$metadata' in val)) {
                throw Error(`Cannot assign a non-model to ${this.$metadata.name} ($metadata prop is missing)`);
            }

            const incomingTypeMeta = val.$metadata;
            if (incomingTypeMeta.name != this.$metadata.itemType.typeDef.name) {
                throw Error(`Type mismatch - attempted to assign a ${incomingTypeMeta.name} to ${this.$metadata.name} (expected a ${this.$metadata.itemType.typeDef.name})`);
            }
            
            if (val instanceof ViewModel) {
                // Already a viewmodel. Do nothing
            } else {
                // Incoming is a Model. Make a ViewModel from it.
                if (ViewModel.typeLookup === null) {
                    throw Error("Static `ViewModel.typeLookup` is not defined. It should get defined in viewmodels.g.ts.");
                }
                var vmCtor = ViewModel.typeLookup[incomingTypeMeta.name];
                val = new vmCtor(val) as unknown as T;
            }

            // $parent and $parentCollection are intentionally protected - 
            // they're just for internal tracking of stuff
            // and probably shouldn't be used in custom code. 
            // So, we'll cast to `any` so we can set them here.
            (val as any).$parent = this.$parent;
            (val as any).$parentCollection = this;

            return val;
        }));
    }

    constructor(
        $metadata: ModelCollectionNavigationProperty,
        $parent: ViewModel
    ) {
        super();
        Object.defineProperties(this, {
            // These properties need to be non-enumerable to avoid them from being looped over
            // during iteration of the array if `for ... in ` is used.
            // We also don't want Vue to bother making them reactive, since they are immutable anyway.
            $metadata: { value: $metadata, enumerable: false, writable: false, configurable: false, },
            $parent: { value: $parent, enumerable: false, writable: false, configurable: false, },
            push: { value: ViewModelCollection.prototype.push, enumerable: false, writable: false, configurable: false, },
        })
    }
    
    // static create<T extends ViewModel>(
    //     $metadata: ModelCollectionNavigationProperty,
    //     $parent: ViewModel
    // ): ViewModelCollection<T> {
    //     // https://blog.simontest.net/extend-array-with-typescript-965cc1134b3
    //     // TL;DR - inheriting from Array doesn't work exactly as you'd expect.
    //     const ret = Object.create(ViewModelCollection.prototype);

    //     Object.defineProperties(ret, {
    //         // These properties need to be non-enumerable to avoid them from being looped over
    //         // during iteration of the array if `for ... in ` is used.
    //         // We also don't want Vue to bother making them reactive, since they are immutable anyway.
    //         $metadata: { value: $metadata, enumerable: false, writable: false, configurable: false, },
    //         $parent: { value: $parent, enumerable: false, writable: false, configurable: false, },
    //     })

    //     return ret;
    // }
}

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

/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
export function defineProps<T extends new() => ViewModel<any, any>>(ctor: T, metadata: ModelType)
{
    Object.defineProperties(ctor.prototype,     
    Object
        .values(metadata.props)
        .reduce(function (descriptors, prop) {
            const propName = prop.name;
            descriptors[propName] = {
                enumerable: true,
                configurable: true,
                get: function(this: InstanceType<T>) {
                    return (this as any).$data[propName]
                },
                set: 
                    prop.type == "model" ?
                        function(this: InstanceType<T>, val: any) {
                            if (val != null) {
                                if (typeof val !== 'object') {
                                    throw `Cannot assign a non-object to ${metadata.name}.${prop.name}`;
                                }
                                else if (!('$metadata' in val)) {
                                    throw `Cannot assign a non-model to ${metadata.name}.${prop.name} ($metadata prop is missing)`;
                                }

                                const incomingTypeMeta = (val as Model).$metadata;
                                if (incomingTypeMeta.name != prop.typeDef.name) {
                                    throw `Type mismatch - attempted to assign a ${incomingTypeMeta.name} to ${metadata.name}.${prop.name}`;
                                }
                                
                                if (val instanceof ViewModel) {
                                    // Already a viewmodel. Do nothing
                                }
                                else {
                                    // Incoming is a Model. Make a ViewModel from it.
                                    // This won't technically be valid according to the types of the properties
                                    // on our generated ViewModels, but we should handle it
                                    // so that input components work really nicely if a component sets the navigation prop.
                                    if (ViewModel.typeLookup === null) {
                                        throw "Static `ViewModel.typeLookup` is not defined. It should get defined in viewmodels.g.ts.";
                                    }
                                    var vmCtor = ViewModel.typeLookup[incomingTypeMeta.name];
                                    val = new vmCtor(val);
                                }

                                if (prop.role == "referenceNavigation") {
                                    // When setting an object, fix up the foreign key using the value pulled from the object
                                    // if it has a value.
                                    const incomingPk = val[prop.principalKey.name];
                                    if (incomingPk) {
                                        (this as Indexable<InstanceType<T>>)[prop.foreignKey.name] = incomingPk;
                                    }
                                }
                            }
                            
                            (this as any).$data[propName] = val;
                        }

                    : prop.role == "collectionNavigation" ?
                        function(this: InstanceType<T>, val: any) {

                            // Usability niceness - make an empty array if the incoming is null.
                            // This shouldn't have any adverse effects that I can think of.
                            // This will cause the viewmodel collections to always be initialized with empty arrays
                            if (val == null) val = [];

                            if (!Array.isArray(val)) {
                                throw `Cannot assign a non-array to ${metadata.name}.${prop.name}`;
                            }
                            const vmc = new ViewModelCollection(prop, this)
                            vmc.push(...val);
                            val = vmc;
                            (this as any).$data[propName] = val;
                        }
                    : function(this: InstanceType<T>, val: any) {
                        // TODO: Implement $emit
                        // this.$emit('valueChanged', prop, value, val);
                        (this as any).$data[propName] = val;
                    }
            }
            return descriptors
        }, {} as PropertyDescriptorMap)
    )
}

export interface ViewModelTypeLookup {
    [name: string]: new(initialData?: any) => ViewModel
}
export interface ListViewModelTypeLookup {
    [name: string]: new() => ListViewModel
}

export type ModelOf<T> = T extends ViewModel<infer TModel> ? TModel : never;

/**
 * Updates the target model with values from the source model.
 * @param target The viewmodel to be updated.
 * @param source The model whose values will be used to perform the update.
 */
export function updateViewModelFromModel<TViewModel extends ViewModel<Model<ModelType>>>(
    target: Indexable<TViewModel>, 
    source: Indexable<ModelOf<TViewModel>>
) {
    const metadata = target.$metadata;

    for (const prop of Object.values(metadata.props)) {
        const propName = prop.name;
        const incomingValue = source[propName];
        const currentValue = target[propName];

        switch (prop.role) {
            case "referenceNavigation":
                if (incomingValue) {
                    if (currentValue == null || currentValue[prop.typeDef.keyProp.name] !== incomingValue[prop.typeDef.keyProp.name]) {
                        // If the current value is null,
                        // or if the current value has a different PK than the incoming value,
                        // we should create a brand new object.

                        // The setter on the viewmodel will handle the conversion to a ViewModel.
                        target[propName] = incomingValue;
                    } else {
                        // `currentValue` is guaranteed to be a ViewModel by virtue of the 
                        // implementations of the setters for referenceNavigation properties on ViewModel instances.
                        currentValue.$loadFromModel(incomingValue);
                    }

                    // Set the foreign key using the PK of the incoming object.
                    // This may end up being redundant when our loop reaches the FK
                    // property, but it will help ensure 100% correctness.
                    target[prop.foreignKey.name] = source[prop.principalKey.name];
                } else {
                    // We allow the existing value of the navigation prop to stick around
                    // if the server didn't send it back.
                    // The setter handling for the foreign key will handle
                    // clearing out the current object if it doesn't match the incoming FK.
                }
                break;
            case "foreignKey":
                if (prop.navigationProp) {
                    /*
                        If there's a navigation property for this FK,
                        we need to null it out if the following are true:
                            - The current value of the navigation prop is non-null
                            - and, the incoming value of the navigation prop IS null
                            - and, tThe incoming value of the FK does not agree with
                                the current PK on the current navigation prop.

                            - OR, the incoming FK is null.
                        
                        This allows us to keep the existing navigation object 
                        even if the server didn't respond with one.
                        However, if the FK has changed to a different value that no longer 
                        agrees with the existing navigation object,
                        we should clear it out to prevent an inconsistent data model.
                    */
                    const incomingObject = source[prop.navigationProp.name];
                    const currentObject = target[prop.navigationProp.name];
                    if (
                        // Clear out the existing navigation prop if the incoming FK is null
                        incomingValue == null

                        // Clear out the existing navigation prop if the incoming FK disagrees with it.
                        || (currentObject != null && incomingObject == null && incomingValue != currentObject[prop.principalKey.name])
                    ) {
                        target[prop.navigationProp.name] = null;
                    }
                }
                target[propName] = incomingValue;
                break;
            case "collectionNavigation":
                if (incomingValue == null) {
                    // No incoming collection was provided. Allow the existing collection to stick around.
                    // Note that this case is different from the incoming value being an empty array,
                    // which should be used to explicitly clear our the existing collection.
                    break;
                } else if (!Array.isArray(incomingValue)) {
                    throw `Expected array for incoming value for ${metadata.name}.${prop.name}`;
                }

                if (!Array.isArray(currentValue) || currentValue.length == 0) {
                    // No existing items. Just take the incoming collection wholesale.
                    // Let the setters on the ViewModel handle the conversion of the contents
                    // into ViewModel instances.
                    target[propName] = incomingValue;
                } else {
                    // There are existing items. We need to surgically merge in the incoming items,
                    // keeping existing ViewModels the same based on keys.
                    const pkName = prop.itemType.typeDef.keyProp.name;
                    const existingItemsMap = new Map();
                    for (let i = 0; i < currentValue.length; i++) {
                        const item = currentValue[i];
                        const itemPk = item[pkName];
                        existingItemsMap.set(itemPk, item);
                    }

                    // Build a new array, using existing items when they exist,
                    // otherwise using the incoming items.
                    const newArray = [];
                    for (let i = 0; i < incomingValue.length; i++) {
                        const incomingItem = incomingValue[i];
                        const incomingItemPk = incomingItem[pkName];
                        const existingItem = existingItemsMap.get(incomingItemPk);
                        if (existingItem) {
                            existingItem.$loadFromModel(incomingItem);
                            newArray.push(existingItem);
                        } else {
                            // No need to $loadFromModel on the incoming item.
                            // The setter for the collection will transform its contents into ViewModels for us.
                            newArray.push(incomingItem);
                        }
                    }
                    
                    // Let the setters on the ViewModel handle the conversion of the contents
                    // into ViewModel instances.
                    target[propName] = newArray;
                }
                break;

            default: 
                target[propName] = incomingValue;
                break;
        }
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
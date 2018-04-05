import Vue from 'vue';
import { AxiosResponse } from 'axios';
import { ModelType, CollectionProperty, PropertyOrName, PropNames } from './metadata';
import { ItemResult, ItemApiState, ModelApiClient } from './api-client';
import { Model } from './model';
import { Indexable } from './util';
/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
export declare function defineProps<T extends new () => ViewModel<any, any>>(ctor: T, metadata: ModelType): void;
export declare abstract class ViewModel<TModel extends Model<ModelType>, TApi extends ModelApiClient<TModel["$metadata"], TModel>> implements Model<TModel["$metadata"]> {
    /** The metadata representing the type of data that this ViewModel handles. */
    readonly $metadata: TModel["$metadata"];
    /** Instance of an API client for the model through which direct, stateless API requests may be made. */
    readonly $apiClient: TApi;
    /**
     * Object which holds all of the data represented by this ViewModel.
     */
    $data: Indexable<TModel>;
    private _pristineDto;
    /**
     * Gets or sets the primary key of the ViewModel's data.
     */
    $primaryKey: any;
    /**
     * Returns true if the values of the savable data properties of this ViewModel
     * have changed since the last load, save, or the last time $isDirty was set to false.
     */
    $isDirty: boolean;
    /**
     * A function for invoking the /get endpoint, and a set of properties about the state of the last call.
     */
    $load: ItemApiState<(id?: string | number | undefined) => Promise<AxiosResponse<ItemResult<TModel>>>, TModel> & ((id?: string | number | undefined) => Promise<AxiosResponse<ItemResult<TModel>>>);
    /**
     * A function for invoking the /save endpoint, and a set of properties about the state of the last call.
     */
    $save: ItemApiState<() => Promise<AxiosResponse<ItemResult<TModel>>>, TModel> & (() => Promise<AxiosResponse<ItemResult<TModel>>>);
    /**
     * A function for invoking the /delete endpoint, and a set of properties about the state of the last call.
     */
    $delete: ItemApiState<() => Promise<AxiosResponse<ItemResult<TModel>>>, TModel> & (() => Promise<AxiosResponse<ItemResult<TModel>>>);
    private _autoSaveState;
    /**
     * Starts auto-saving of the instance's data properties when changes occur.
     * Only properties which will be sent in save requests are watched -
     * navigation properties are not considered.
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param wait Time in milliseconds to debounce saves for
     * @param predicate A function that will be called before saving that can return false to prevent a save.
     */
    $startAutoSave(vue: Vue, wait?: number, predicate?: (viewModel: this) => boolean): void;
    /** Stops auto-saving if it is currently enabled. */
    $stopAutoSave(): void;
    /**
     * Returns a string representation of the object, or one of its properties, suitable for display.
     * @param prop If provided, specifies a property whose value will be displayed.
     * If omitted, the whole object will be represented.
     */
    $display(prop?: PropertyOrName<TModel["$metadata"]>): string | null;
    /**
     * Creates a new instance of an item for the specified child collection, adds it to that collection, and returns the item.
     * For class collections, this will be a valid implementation of the corresponding model interface.
     * For non-class collections, this will be null.
     * @param prop The name of the collection property, or the metadata representing it.
     */
    $addChild(prop: CollectionProperty | PropNames<TModel["$metadata"], CollectionProperty>): Model<ModelType> | null;
    constructor(
        /** The metadata representing the type of data that this ViewModel handles. */
        $metadata: TModel["$metadata"], 
        /** Instance of an API client for the model through which direct, stateless API requests may be made. */
        $apiClient: TApi, initialData?: TModel);
}

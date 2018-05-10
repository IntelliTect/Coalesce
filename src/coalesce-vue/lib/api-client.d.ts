declare module "vue/types/vue" {
    interface VueConstructor<V extends Vue = Vue> {
        util: {
            defineReactive: (obj: any, key: string, val: any, setter: Function | null, shallow: boolean) => void;
        };
    }
}
declare module "axios" {
    interface AxiosInstance {
        delete<T = any>(url: string, config?: AxiosRequestConfig): AxiosPromise<T>;
        head<T = any>(url: string, config?: AxiosRequestConfig): AxiosPromise<T>;
    }
}
import { ModelType, Method, Service, ApiRoutedType, DataSourceType, Value, CollectionValue, VoidValue } from './metadata';
import { Model, DataSource } from './model';
import { AxiosResponse, AxiosRequestConfig, AxiosInstance } from 'axios';
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
export interface ListResult<T = any> extends ApiResult {
    list?: T[];
    page: number;
    pageSize: number;
    pageCount: number;
    totalCount: number;
}
export interface DataSourceParameters {
    /** An string that the server may use to include/exclude certain data from the results. See Coalesce's full documentation for details. */
    includes?: string | null;
    /**
     * A data source instance that will be used to load the data.
     * Classes are found in `models.g.ts` as `<ModelName>.DataSources.<DataSourceName>`, e.g. `Person.DataSources.WithRelations`.
     */
    dataSource?: DataSource<DataSourceType> | null;
}
export declare class DataSourceParameters {
    constructor();
}
export interface FilterParameters extends DataSourceParameters {
    /** A search term to search by. Searching behavior is determined by the server. */
    search?: string | null;
    /** A collection of key-value pairs to filter by. Behavior is dependent on the type of each field, see Coalesce's full documentation for details. */
    filter?: {
        [fieldName: string]: string;
    } | null;
}
export declare class FilterParameters extends DataSourceParameters {
    constructor();
}
export interface ListParameters extends FilterParameters {
    /** The page of data to request, starting at 1. */
    page?: number | null;
    /** The number of items per page to request. */
    pageSize?: number | null;
    /**
     * The name of a field to order the results by.
     *  If this and `orderByDescending` are blank, default ordering determined by the server will be used.
     * */
    orderBy?: string | null;
    /**
     * The name of a field to order the results by in descending order.
     * If this and `orderBy` are blank, default ordering determined by the server will be used.
     * */
    orderByDescending?: string | null;
    /**
     * A list of field names to request. The results returned will only have these fields populated - all other fields will be null.
     */
    fields?: string[] | null;
}
export declare class ListParameters extends FilterParameters {
    constructor();
}
export declare type AxiosItemResult<T> = AxiosResponse<ItemResult<T>>;
export declare type AxiosListResult<T> = AxiosResponse<ListResult<T>>;
export declare type ItemResultPromise<T> = Promise<AxiosResponse<ItemResult<T>>>;
export declare type ListResultPromise<T> = Promise<AxiosResponse<ListResult<T>>>;
export declare type ApiResultPromise<T> = Promise<AxiosItemResult<T> | AxiosListResult<T>>;
/** Axios instance to be used by all Coalesce API requests. Can be configured as needed. */
export declare const AxiosClient: AxiosInstance;
export declare type ItemApiReturnType<T extends (this: null, ...args: any[]) => ItemResultPromise<any>> = ReturnType<T> extends void ? void : ReturnType<T> extends ItemResultPromise<infer R> ? R : any;
export declare type ListApiReturnType<T extends (this: null, ...args: any[]) => ListResultPromise<any>> = ReturnType<T> extends ListResultPromise<infer S> ? S : any;
export declare type ApiCallerConcurrency = "cancel" | "disallow" | "allow";
export declare class ApiClient<T extends ApiRoutedType> {
    $metadata: T;
    constructor($metadata: T);
    /** Cancellation token to inject into the next request. */
    private _nextCancelToken;
    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "item" indicating that the API endpoint returns an ItemResult<T>
     * @param invokerFactory method that will return a function that can be used to call the API. The signature of the returned function will be the call signature of the wrapper.
     */
    $makeCaller<TCall extends (this: any, ...args: any[]) => ItemResultPromise<any>>(resultType: "item", invokerFactory: (client: this) => TCall): ItemApiState<TCall, ItemApiReturnType<TCall>> & TCall;
    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "list" indicating that the API endpoint returns an ListResult<T>
     * @param invokerFactory method that will return a function that can be used to call the API. The signature of the returned function will be the call signature of the wrapper.
     */
    $makeCaller<TCall extends (this: any, ...args: any[]) => ListResultPromise<any>>(resultType: "list", invokerFactory: (client: this) => TCall): ListApiState<TCall, ListApiReturnType<TCall>> & TCall;
    /**
     * Maps the given method parameters to values suitable for transport.
     * @param method The method whose parameters need mapping
     * @param params The values of the parameter to map
     */
    protected $mapParams(method: Method, params: {
        [paramName: string]: any;
    }): {
        [paramName: string]: any;
    };
    /**
     * Combines the input into a single `AxiosRequestConfig` object.
     * @param parameters The Coalesce parameters for the standard API endpoints.
     * @param config A full `AxiosRequestConfig` to merge in.
     * @param queryParams An object with an additional querystring parameters.
     */
    protected $options(parameters?: ListParameters | FilterParameters | DataSourceParameters, config?: AxiosRequestConfig, queryParams?: any): AxiosRequestConfig;
    private $serializeParams(parameters?);
    protected $hydrateItemResult<TResult>(value: AxiosItemResult<TResult>, metadata: Value | VoidValue): AxiosResponse<ItemResult<TResult>>;
    protected $hydrateListResult<TResult>(value: AxiosListResult<TResult>, metadata: CollectionValue): AxiosResponse<ListResult<TResult>>;
}
export declare class ModelApiClient<TModel extends Model<ModelType>> extends ApiClient<TModel["$metadata"]> {
    get(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig): ItemResultPromise<TModel>;
    list(parameters?: ListParameters, config?: AxiosRequestConfig): ListResultPromise<TModel>;
    count(parameters?: FilterParameters, config?: AxiosRequestConfig): ItemResultPromise<number>;
    save(item: TModel, parameters?: DataSourceParameters, config?: AxiosRequestConfig): ItemResultPromise<TModel>;
    delete(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig): ItemResultPromise<TModel>;
    /** Value metadata for handling ItemResult returns from the standard API endpoints. */
    private $itemValueMeta;
    /** Value metadata for handling ListResult returns from the standard API endpoints. */
    private $collectionValueMeta;
}
export declare abstract class ServiceApiClient<TMeta extends Service> extends ApiClient<TMeta> {
}
export declare abstract class ApiState<TCall extends (this: null, ...args: any[]) => ApiResultPromise<TResult>, TResult> extends Function {
    private readonly apiClient;
    private readonly invoker;
    /** True if a request is currently pending. */
    isLoading: boolean;
    /** True if the previous request was successful. */
    wasSuccessful: boolean | null;
    /** Error message returned by the previous request. */
    message: string | null;
    private _concurrencyMode;
    /**
     * Function that can be called to cancel a pending request.
    */
    cancel(): void;
    /**
     * Set the concurrency mode for this API caller. Default is "disallow".
     * @param mode Behavior for when a request is made while there is already an outstanding request.
     *
     * "cancel" - cancel the outstanding request first.
     *
     * "disallow" - throw an error.
     *
     * "allow" - permit the second request to be made. The ultimate state of the state fields may not be representative of the last request made.
     */
    setConcurrency(mode: ApiCallerConcurrency): this;
    /**
     * Get or set the concurrency mode for this API caller. Default is "disallow".
     * @param mode Behavior for when a request is made while there is already an outstanding request.
     *
     * "cancel" - cancel the outstanding request first.
     *
     * "disallow" - throw an error.
     *
     * "allow" - permit the second request to be made. The ultimate state of the state fields may not be representative of the last request made.
     */
    concurrencyMode: ApiCallerConcurrency;
    private _cancelToken;
    private _callbacks;
    /**
     * Attach a callback to be invoked when the request to this endpoint succeeds.
     * @param onFulfilled A callback to be called when a request to this endpoint succeeds.
     */
    onFulfilled(callback: (this: any, state: this) => void): this;
    /**
     * Attach a callback to be invoked when the request to this endpoint fails.
     * @param onFulfilled A callback to be called when a request to this endpoint fails.
     */
    onRejected(callback: (this: any, state: this) => void): this;
    protected abstract setResponseProps(data: ApiResult): void;
    invoke: TCall;
    private _invokeInternal(thisArg, args);
    protected _makeReactive(): void;
    constructor(apiClient: ApiClient<any>, invoker: TCall);
}
export declare class ItemApiState<TCall extends (this: null, ...args: any[]) => ItemResultPromise<TResult>, TResult> extends ApiState<TCall, TResult> {
    /** Validation issues returned by the previous request. */
    validationIssues: ValidationIssue[] | null;
    /** Principal data returned by the previous request. */
    result: TResult | null;
    constructor(apiClient: ApiClient<any>, invoker: TCall);
    protected setResponseProps(data: ItemResult<TResult>): void;
}
export declare class ListApiState<TCall extends (this: null, ...args: any[]) => ListResultPromise<TResult>, TResult> extends ApiState<TCall, TResult> {
    /** Page number returned by the previous request. */
    page: number | null;
    /** Page size returned by the previous request. */
    pageSize: number | null;
    /** Page count returned by the previous request. */
    pageCount: number | null;
    /** Total Count returned by the previous request. */
    totalCount: number | null;
    /** Principal data returned by the previous request. */
    result: TResult[] | null;
    constructor(apiClient: ApiClient<any>, invoker: TCall);
    protected setResponseProps(data: ListResult<TResult>): void;
}

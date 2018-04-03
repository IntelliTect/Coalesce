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
import { ModelType, ClassType, Method } from './metadata';
import { Model } from './model';
import { AxiosPromise, AxiosResponse, AxiosRequestConfig, CancelToken, AxiosInstance } from 'axios';
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
    includes?: string;
    dataSource?: never;
}
export interface FilterParameters extends DataSourceParameters {
    search?: string;
    filter?: {
        [fieldName: string]: string;
    };
}
export interface ListParameters extends FilterParameters {
    page?: number;
    pageSize?: number;
    orderBy?: string;
    orderByDescending?: string;
    fields?: string[];
}
export declare type ApiResponse<T> = Promise<AxiosResponse<T>>;
export declare type AxiosItemResult<T> = AxiosResponse<ItemResult<T>>;
export declare type AxiosListResult<T> = AxiosResponse<ListResult<T>>;
export declare type ItemResultPromise<T> = Promise<AxiosResponse<ItemResult<T>>>;
export declare type ListResultPromise<T> = Promise<AxiosResponse<ListResult<T>>>;
export declare type ApiResultPromise<T> = Promise<AxiosItemResult<T> | AxiosListResult<T>>;
/** Axios instance to be used by all Coalesce API requests. Can be configured as needed. */
export declare const AxiosClient: AxiosInstance;
export declare type ItemApiReturnType<T extends (this: null, ...args: any[]) => ItemResultPromise<any>> = ReturnType<T> extends void ? void : ReturnType<T> extends ItemResultPromise<infer R> ? R : any;
export declare type ListApiReturnType<T extends (this: null, ...args: any[]) => ListResultPromise<any>> = ReturnType<T> extends ListResultPromise<infer S> ? S : any;
export declare class ApiClient<T extends Model<ClassType>> {
    $metadata: ModelType;
    constructor($metadata: ModelType);
    /** Injects a cancellation token into the next request. */
    private _nextCancelToken;
    get(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig): Promise<AxiosResponse<ItemResult<T>>>;
    list(parameters?: ListParameters, config?: AxiosRequestConfig): Promise<AxiosResponse<ListResult<T>>>;
    count(parameters?: FilterParameters, config?: AxiosRequestConfig): AxiosPromise<AxiosResponse<ItemResult<number>>>;
    save(item: T, parameters?: DataSourceParameters, config?: AxiosRequestConfig): Promise<AxiosResponse<ItemResult<T>>>;
    delete(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig): Promise<AxiosResponse<ItemResult<T>>>;
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
    $makeCaller<TCall extends (this: null, ...args: any[]) => ListResultPromise<any>>(resultType: "list", invokerFactory: (client: this) => TCall): ListApiState<TCall, ListApiReturnType<TCall>> & TCall;
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
    protected $options(parameters?: ListParameters | FilterParameters | DataSourceParameters, config?: AxiosRequestConfig, queryParams?: any): {
        cancelToken: CancelToken | null;
    } & AxiosRequestConfig & {
        params: any;
    };
    private $objectify(parameters?);
    protected $hydrateItemResult<TResult>(value: AxiosItemResult<TResult>, metadata: ClassType): AxiosResponse<ItemResult<TResult>>;
    protected $hydrateListResult<TResult>(value: AxiosListResult<TResult>, metadata: ClassType): AxiosResponse<ListResult<TResult>>;
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
    /**
     * Function that can be called to cancel a pending request.
    */
    cancel(): void;
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
    constructor(apiClient: ApiClient<any>, invoker: TCall);
}
export declare class ItemApiState<TCall extends (this: null, ...args: any[]) => ItemResultPromise<TResult>, TResult> extends ApiState<TCall, TResult> {
    /** Validation issues returned by the previous request. */
    validationIssues: ValidationIssue[] | null;
    /** Principal data returned by the previous request. */
    result: TResult | null;
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
    protected setResponseProps(data: ListResult<TResult>): void;
}

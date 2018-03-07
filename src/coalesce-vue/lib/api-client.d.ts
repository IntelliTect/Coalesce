declare module "vue/types/vue" {
    interface VueConstructor<V extends Vue = Vue> {
        util: {
            defineReactive: (obj: any, key: string, val: any, setter: Function | null, shallow: boolean) => void;
        };
    }
}
import { ModelType, ClassType } from './metadata';
import { Model } from './model';
import { AxiosPromise, AxiosResponse, AxiosError, AxiosRequestConfig, Canceler, CancelToken, AxiosInstance } from 'axios';
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
    $makeCaller<TCall extends (this: null, ...args: any[]) => ItemResultPromise<T>>(resultType: "item", invokerFactory: (client: this) => TCall): ItemApiState<TCall, T> & TCall;
    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "list" indicating that the API endpoint returns an ListResult<T>
     * @param invokerFactory method that will return a function that can be used to call the API. The signature of the returned function will be the call signature of the wrapper.
     */
    $makeCaller<TCall extends (this: null, ...args: any[]) => ListResultPromise<T>>(resultType: "list", invokerFactory: (client: this) => TCall): ListApiState<TCall, T> & TCall;
    protected $options(parameters?: ListParameters | FilterParameters | DataSourceParameters, config?: AxiosRequestConfig, queryParams?: any): {
        cancelToken: CancelToken | null;
    } & AxiosRequestConfig & {
        params: any;
    };
    private $objectify(parameters?);
    private $hydrateItemResult(value);
    private $hydrateListResult(value);
}
export declare abstract class ApiState<TCall extends (this: null, ...args: any[]) => ApiResultPromise<T>, T extends Model<ClassType>> extends Function {
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
    cancel: Canceler | null;
    private _callbacks;
    /**
     * Attach a callback to be invoked when the request to this endpoint succeeds.
     * @param onFulfilled A callback to be called when a request to this endpoint succeeds.
     */
    onFulfilled(callback: (state: this) => void): this;
    /**
     * Attach a callback to be invoked when the request to this endpoint fails.
     * @param onFulfilled A callback to be called when a request to this endpoint fails.
     */
    onRejected(callback: (state: this) => void): this;
    abstract setResponseProps(data: ApiResult): void;
    invoke: TCall;
    _invokeInternal(): Promise<AxiosError | AxiosResponse<ItemResult<T>> | AxiosResponse<ListResult<T>>>;
    constructor(apiClient: ApiClient<T>, invoker: TCall);
}
export declare class ItemApiState<TCall extends (this: null, ...args: any[]) => ItemResultPromise<T>, T extends Model<ClassType>> extends ApiState<TCall, T> {
    /** Validation issues returned by the previous request. */
    validationIssues: ValidationIssue[] | null;
    /** Principal data returned by the previous request. */
    result: T | null;
    setResponseProps(data: ItemResult<T>): void;
}
export declare class ListApiState<TCall extends (this: null, ...args: any[]) => ListResultPromise<T>, T extends Model<ClassType>> extends ApiState<TCall, T> {
    /** Page number returned by the previous request. */
    page: number | null;
    /** Page size returned by the previous request. */
    pageSize: number | null;
    /** Page count returned by the previous request. */
    pageCount: number | null;
    /** Total Count returned by the previous request. */
    totalCount: number | null;
    /** Principal data returned by the previous request. */
    result: T[] | null;
    setResponseProps(data: ListResult<T>): void;
}

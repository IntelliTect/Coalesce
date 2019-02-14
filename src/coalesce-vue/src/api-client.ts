
import Vue from 'vue';

// Undocumented (but exposed) vue method for making properties reactive.
declare module "vue/types/vue" {
    interface VueConstructor {
        util: {
            defineReactive: (obj: any, key: string, val: any, setter?: Function | null, shallow?: boolean) => void
        }
    }
}

// Works around https://github.com/axios/axios/issues/1365 (PR https://github.com/axios/axios/pull/1401)
declare module "axios" {
    export interface AxiosInstance {
      delete<T = any>(url: string, config?: AxiosRequestConfig): AxiosPromise<T>;
      head<T = any>(url: string, config?: AxiosRequestConfig): AxiosPromise<T>;
    }
}

import { ModelType, ClassType, Method, Service, ApiRoutedType, DataSourceType, Value, ModelValue, CollectionValue, VoidValue, ItemMethod, ListMethod } from './metadata'
import { Model, convertToModel, mapToDto, mapValueToDto, DataSource, convertValueToModel } from './model'
import { OwnProps, Indexable } from './util'

import axios, { AxiosPromise, AxiosResponse, AxiosError, AxiosRequestConfig, Canceler, CancelTokenSource, CancelToken, AxiosInstance, Cancel} from 'axios'
import * as qs from 'qs'


/* Api Response Objects */

export interface ApiResult {
    wasSuccessful: boolean
    message?: string
}

export interface ValidationIssue {
    property: string
    issue: string
}

export interface ItemResult<T = any> extends ApiResult {
    object?: T
    validationIssues?: ValidationIssue[]
}

export interface ListResult<T = any> extends ApiResult {
    list?: T[]
    page: number
    pageSize: number
    pageCount: number
    totalCount: number
}


/* Api Parameter Objects */

export interface DataSourceParameters {
    /** An string that the server may use to include/exclude certain data from the results. See Coalesce's full documentation for details. */
    includes?: string | null

    /** 
     * A data source instance that will be used to load the data. 
     * Classes are found in `models.g.ts` as `<ModelName>.DataSources.<DataSourceName>`, e.g. `Person.DataSources.WithRelations`.
     */
    dataSource?: DataSource<DataSourceType> | null
}
export class DataSourceParameters {
    constructor() {
        this.includes = null;
        this.dataSource = null;
    }
}

export interface FilterParameters extends DataSourceParameters {
    /** A search term to search by. Searching behavior is determined by the server. */
    search?: string | null

    /** A collection of key-value pairs to filter by. Behavior is dependent on the type of each field, see Coalesce's full documentation for details. */
    filter?: { [fieldName: string]: string } | null
}
export class FilterParameters extends DataSourceParameters {
    constructor() {
        super();
        this.search = null;
        this.filter = null;
    }
}

export interface ListParameters extends FilterParameters {
    /** The page of data to request, starting at 1. */
    page?: number | null

    /** The number of items per page to request. */
    pageSize?: number | null

    /** 
     * The name of a field to order the results by.
     *  If this and `orderByDescending` are blank, default ordering determined by the server will be used. 
     * */
    orderBy?: string | null
    /** 
     * The name of a field to order the results by in descending order. 
     * If this and `orderBy` are blank, default ordering determined by the server will be used. 
     * */

    orderByDescending?: string | null

    /**
     * A list of field names to request. The results returned will only have these fields populated - all other fields will be null.
     */
    fields?: string[] | null
}
export class ListParameters extends FilterParameters {
    constructor() {
        super();
        this.page = 1;
        this.pageSize = 25;
        this.orderBy = null;
        this.orderByDescending = null;
        this.fields = null;
    }
}



export type AxiosItemResult<T> = AxiosResponse<ItemResult<T>>
export type AxiosListResult<T> = AxiosResponse<ListResult<T>>
export type ItemResultPromise<T> = Promise<AxiosResponse<ItemResult<T>>>
export type ListResultPromise<T> = Promise<AxiosResponse<ListResult<T>>>
// ApiResultPromise must be the inner union of the promise types, not the outer union of two promises.
// Otherwise, typescript doesn't like us calling a function with a union return type. For some reason.
export type ApiResultPromise<T> = Promise<AxiosItemResult<T> | AxiosListResult<T>>

/** Axios instance to be used by all Coalesce API requests. Can be configured as needed. */
export const AxiosClient = axios.create()
AxiosClient.defaults.baseURL = '/api'

export type ItemApiReturnType<T extends (this: null, ...args: any[]) => ItemResultPromise<any>> 
    = ReturnType<T> extends void ? void
    : ReturnType<T> extends ItemResultPromise<infer R> ? R 
    : any;

export type ListApiReturnType<T extends (this: null, ...args: any[]) => ListResultPromise<any>> 
    = ReturnType<T> extends ListResultPromise<infer S> ? S
    : any;

type AnyApiReturnType<T extends (this: null, ...args: any[]) => ApiResultPromise<any>> 
    = ReturnType<T> extends ApiResultPromise<infer S> ? S : any;

export type ApiCallerConcurrency = "cancel" | "disallow" | "allow" | "debounce"

export class ApiClient<T extends ApiRoutedType> {

    constructor(public $metadata: T) {
    }

    /** Cancellation token to inject into the next request. */
    private _nextCancelToken: CancelTokenSource | null = null

    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "item" indicating that the API endpoint returns an ItemResult<T>
     * @param invokerFactory method that will call the API. The signature of the function, minus the apiClient parameter, will be the call signature of the wrapper.
     */
    $makeCaller<TArgs extends any[], TResult>(
        resultType: "item" | ItemMethod | ((methods: T["methods"]) => ItemMethod),
        invoker: TInvoker<TArgs, ItemResultPromise<TResult>, this>
    ): ItemApiState<TArgs, TResult, this> & TCall<TArgs, ItemResultPromise<TResult>>

    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "list" indicating that the API endpoint returns an ListResult<T>
     * @param invokerFactory method that will call the API. The signature of the function, minus the apiClient parameter, will be the call signature of the wrapper.
     */
    $makeCaller<TArgs extends any[], TResult>(
        resultType: "list" | ListMethod | ((methods: T["methods"]) => ListMethod),
        invoker: TInvoker<TArgs, ListResultPromise<TResult>, this>
    ): ListApiState<TArgs, TResult, this> & TCall<TArgs, ListResultPromise<TResult>>

    $makeCaller<TArgs extends any[], TResult>(
        resultType: "item" | "list" | Method | ((methods: T["methods"]) => Method),
        invoker: TInvoker<TArgs, ApiResultPromise<TResult>, this>
    ): ApiState<TArgs, TResult, this> & TCall<TArgs, TResult>{
        
        let meta: Method | undefined = undefined;
        if (typeof resultType === "function") {
            meta = resultType(this.$metadata.methods)
            resultType = meta.transportType;
        } else if (typeof resultType === "object") {
            meta = resultType
            resultType = resultType.transportType;
        }
        
        // This is basically all just about resolving the overloads. 
        // We use `any` because the types get really messy if you try to handle both types at once.

        var instance;
        switch (resultType){
            case "item": 
                instance = new ItemApiState<TArgs, TResult, this>(this, invoker);
                break;
            case "list": 
                instance = new ListApiState<TArgs, TResult, this>(this, invoker as any);
                break;
            default: throw `Unknown result type ${resultType}`
        }

        // Set this on the instance if we have it.
        instance.metadata = meta;

        return instance as any;
    }

    public $invoke(
        method: ItemMethod,
        params: { [P in keyof Method["params"]]: any; },
        config?: AxiosRequestConfig
    ): AxiosPromise<ItemResult<any>>;

    public $invoke(
        method: ListMethod,
        params: { [P in keyof Method["params"]]: any; },
        config?: AxiosRequestConfig
    ): AxiosPromise<ListResult<any>>;
    
    /**
     * Invoke the specified method using the provided set of parameters.
     * @param method The metadata of the API  method to invoke
     * @param params The parameters to provide to the API method.
     * @param config A full `AxiosRequestConfig` to merge in.
     */
    public $invoke(
        method: Method,
        params: { [P in keyof Method["params"]]: any; },
        config?: AxiosRequestConfig
    ) {
        params = this.$mapParams(method, params);
        const hasBody = method.httpMethod != "GET" && method.httpMethod != "DELETE";
        
        return AxiosClient
            .request({
                method: method.httpMethod,
                url: `/${this.$metadata.controllerRoute}/${method.name}`,
                data: hasBody 
                    ? qs.stringify(params) 
                    : undefined,
                ...this.$options(undefined, config, !hasBody ? params : undefined)
            }
            )
            .then(r => {
                switch (method.transportType) {
                    case "item": return this.$hydrateItemResult(r, method.return)
                    case "list": return this.$hydrateListResult(r, method.return)
                }
            })
    }

    /**
     * Maps the given method parameters to values suitable for transport.
     * @param method The method whose parameters need mapping
     * @param params The values of the parameter to map
     */
    protected $mapParams(
        method: Method,
        params: { [paramName: string]: any }
    ) {
        const formatted: { [paramName: string]: any } = {};
        for (var paramName in params) {
            const paramMeta = method.params[paramName];
            formatted[paramName] = mapValueToDto(params[paramName], paramMeta)
        }
        return formatted;
    }

    /**
     * Combines the input into a single `AxiosRequestConfig` object.
     * @param parameters The Coalesce parameters for the standard API endpoints.
     * @param config A full `AxiosRequestConfig` to merge in.
     * @param queryParams An object with an additional querystring parameters.
     */
    protected $options(
        parameters?: ListParameters | FilterParameters | DataSourceParameters, 
        config?: AxiosRequestConfig,
        queryParams?: any
    ): AxiosRequestConfig {
        return {
            cancelToken: (this._nextCancelToken && this._nextCancelToken!.token) || undefined, 
            ...config, 

            // Merge standard Coalesce params with general configured params if there are any.
            // Params come last to overwrite config.params with our merged params object.
            params: { 
                ...queryParams,
                ...(config && config.params ? config.params : null), 
                ...this.$serializeParams(parameters)
            }
        }
    }

    private $serializeParams(parameters?: ListParameters | FilterParameters | DataSourceParameters) {
        if (!parameters) return null

        // Assume the widest type, which is ListParameters.
        var wideParams = parameters as Partial<ListParameters>;

        // The list of 'simple' params where we just pass along the exact value.
        var simpleParams = [
            'includes', 'search', 'page', 'pageSize', 'orderBy', 'orderByDescending'
        ] as Array<keyof typeof wideParams>;
        
        // Map all the simple params to `paramsObject`
        var paramsObject = simpleParams.reduce((obj, key) => {
            if (key in wideParams) obj[key] = wideParams[key];
            return obj;
        }, {} as any);

        // Map the 'filter' object, ensuring all values are strings.
        const filter = wideParams.filter;
        if (typeof filter == 'object' && filter) {
            for (var key in filter) {
                if (filter[key] !== undefined) {
                    paramsObject["filter." + key] = filter[key];
                }
            }
        }

        if (Array.isArray(wideParams.fields)) {
            paramsObject.fields = wideParams.fields.join(',')
        }

        // Map the data source and its params
        const dataSource = wideParams.dataSource as Indexable<typeof wideParams.dataSource>
        if (dataSource) {
            // Add the data source name
            paramsObject["dataSource"] = dataSource.$metadata.name;
            var paramsMeta = dataSource.$metadata.params;

            // Add the data source parameters.
            // Note that we use "dataSource.{paramName}", not a nested object. 
            // This is what the model binder expects.
            for (var paramName in paramsMeta) {
                const paramMeta = paramsMeta[paramName];
                if (paramName in dataSource) {
                    const paramValue = dataSource[paramName];
                    paramsObject["dataSource." + paramMeta.name] = mapValueToDto(paramValue, paramMeta)
                }
            }
        }

        return paramsObject;
    }

    protected $hydrateItemResult<TResult>(value: AxiosItemResult<TResult>, metadata: Value | VoidValue) {
        // Do nothing for void returns - there will be no object.
        if (metadata.type !== "void") {
            // This function is NOT PURE - we mutate the result object on the response.
            value.data.object = convertValueToModel(value.data.object, metadata)
        }
        return value;
    }

    protected $hydrateListResult<TResult>(value: AxiosListResult<TResult>, metadata: CollectionValue) {
        // This function is NOT PURE - we mutate the result object on the response.
        value.data.list = convertValueToModel(value.data.list, metadata)
        return value;
    }
}

export class ModelApiClient<TModel extends Model<ModelType>> extends ApiClient<TModel["$metadata"]> {

    // TODO: should the standard set of endpoints be prefixed with '$'?

    public get(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig): ItemResultPromise<TModel> {
        return AxiosClient
            .get(
                `/${this.$metadata.controllerRoute}/get/${id}`, 
                this.$options(parameters, config)
            )
            .then<AxiosItemResult<TModel>>(r => this.$hydrateItemResult(r, this.$itemValueMeta))
    }
    
    public list(parameters?: ListParameters, config?: AxiosRequestConfig): ListResultPromise<TModel> {
        return AxiosClient
            .get(
                `/${this.$metadata.controllerRoute}/list`, 
                this.$options(parameters, config)
            )
            .then<AxiosListResult<TModel>>(r => this.$hydrateListResult(r, this.$collectionValueMeta))
    }
    
    public count(parameters?: FilterParameters, config?: AxiosRequestConfig): ItemResultPromise<number> {
        return AxiosClient
            .get<ItemResult<number>>(
                `/${this.$metadata.controllerRoute}/count`, 
                this.$options(parameters, config)
            )
    }
    
    public save(item: TModel, parameters?: DataSourceParameters, config?: AxiosRequestConfig): ItemResultPromise<TModel> {
        return AxiosClient
            .post(
                `/${this.$metadata.controllerRoute}/save`,
                qs.stringify(mapToDto(item)),
                this.$options(parameters, config)
            )
            .then<AxiosItemResult<TModel>>(r => this.$hydrateItemResult(r, this.$itemValueMeta))
    }
    
    public delete(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig): ItemResultPromise<TModel> {
        return AxiosClient
            .post(
                `/${this.$metadata.controllerRoute}/delete/${id}`,
                null,
                this.$options(parameters, config)
            )
            .then<AxiosItemResult<TModel>>(r => this.$hydrateItemResult(r, this.$itemValueMeta))
    }
    
    /** Value metadata for handling ItemResult returns from the standard API endpoints. */
    private $itemValueMeta = Object.freeze(<ModelValue>{
        name: "object", displayName: "",
        type: "model",
        role: "value",
        typeDef: this.$metadata,
    })

    /** Value metadata for handling ListResult returns from the standard API endpoints. */
    private $collectionValueMeta = Object.freeze(<CollectionValue>{
        name: "list", displayName: "",
        type: "collection",
        role: "value",
        itemType: this.$itemValueMeta,
    })
}

export abstract class ServiceApiClient<TMeta extends Service> extends ApiClient<TMeta> {
    
}

export type TInvoker<TArgs extends any[], TReturn, TClient extends ApiClient<any>> = (this: any, client: TClient, ...args: TArgs) => TReturn
export type TCall<TArgs extends any[], TReturn> = (this: any, ...args: TArgs) => TReturn

export abstract class ApiState<TArgs extends any[], TResult, TClient extends ApiClient<any>> extends Function {

    /** The metadata of the method being called, if it was provided. */
    metadata?: Method

    /** True if a request is currently pending. */
    isLoading: boolean = false
    
    /** True if the previous request was successful. */
    wasSuccessful: boolean | null = null
    
    /** Error message returned by the previous request. */
    message: string | null = null

    private _concurrencyMode: ApiCallerConcurrency = "disallow"

    /** 
     * Function that can be called to cancel a pending request.
    */
    cancel() {
        if (this._cancelToken) {
            this._cancelToken.cancel();
            this.isLoading = false;
        }
    }

    /**
     * Set the concurrency mode for this API caller. Default is "disallow".
     * @param mode Behavior for when a request is made while there is already an outstanding request.
     * 
     * "cancel" - Cancel the outstanding request first. 
     * 
     * "debounce" - if a request is made while one is outstanding, enqueue it to start after the outstanding 
     * request is done. If another request is made while one is already enqueued, the enqueued request is abandoned
     * and replaced by the last request that was made.
     * 
     * "disallow" - Throw an error. 
     * 
     * "allow" - Permit the second request to be made. The ultimate values of the state fields may not be representative of the last request made.
     */
    setConcurrency(mode: ApiCallerConcurrency) {
        // This method exists as a way to configure this in a chainable way when instantiating API callers.
        this._concurrencyMode = mode;
        return this;
    }

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
    get concurrencyMode() { return this._concurrencyMode };
    set concurrencyMode(val: ApiCallerConcurrency) { this.setConcurrency(val) };


    // Undefined initially to prevent unneeded reactivity
    private _cancelToken: CancelTokenSource | undefined;

    // Frozen to prevent unneeded reactivity.
    private _callbacks = Object.freeze<{
        onFulfilled: Array<Function>, 
        onRejected: Array<Function>
    }>({onFulfilled: [], onRejected: []})

    /**
     * Attach a callback to be invoked when the request to this endpoint succeeds.
     * @param callback A callback to be called when a request to this endpoint succeeds.
     */
    onFulfilled(callback: (this: any, state: this) => void): this {
        this._callbacks.onFulfilled.push(callback)
        return this;
    }

    /**
     * Attach a callback to be invoked when the request to this endpoint fails.
     * @param callback A callback to be called when a request to this endpoint fails.
     */
    onRejected(callback: (this: any, state: this) => void): this {
        this._callbacks.onRejected.push(callback)
        return this;
    }

    protected abstract setResponseProps(data: ApiResult): void

    public invoke!: this

    private _debounceSignal: { promise: Promise<void>, resolve: () => void, reject: () => void } | null = null;
    private async _invokeInternal(thisArg: any, ...args: TArgs) {
        if (this.isLoading) {
            if (this._concurrencyMode === "disallow") {
                throw `Request is already pending for invoker ${this.invoker.toString()}`
            } else if (this._concurrencyMode === "cancel") {
                this.cancel()
            } else if (this._concurrencyMode === "debounce") {
                // If there's already a pending debounced request,
                // reject it, and then create a new promise and await a successful signal.
                if (this._debounceSignal) {
                    this._debounceSignal.reject()
                }

                const signal: any = {};
                signal.promise = new Promise((resolve, reject) => {
                    signal.resolve = resolve;
                    signal.reject = reject;
                })
                this._debounceSignal = signal;
                
                // Await completion of the current outstanding request,
                // or until another call is made while we're still pending.
                try {
                    await signal.promise;
                } catch {
                    // Similar to the "cancel" mode,
                    // aborted requests are not thrown as rejected promises,
                    // but instead as a fulfilled promise with a void resolved value.
                    return void 0;
                }
            }
        }


        // Change no state except `isLoading` until after the promise is resolved.
        // this.wasSuccessful = null
        // this.message = null
        this.isLoading = true

        // Inject a cancellation token into the request.
        var promise: ApiResultPromise<TResult>
        try {
            const token = (this.apiClient as any)._nextCancelToken = axios.CancelToken.source()
            this._cancelToken = token
            const resp = await (this.invoker as any).apply(thisArg, [this.apiClient, ...args])

            const data = resp.data
            delete this._cancelToken
            this.setResponseProps(data)

            this._callbacks.onFulfilled.forEach(cb => cb.apply(thisArg, [this]))

            this.isLoading = false

            // We have to maintain the shape of the promise of the stateless invoke method.
            // This means we can't re-shape ourselves into a Promise<ApiState<T>> with `return fn` here.
            // The reason for this is that we can't change the return type of TCall while maintaining 
            // the param signature (unless we required a full, explicit type annotation as a type parameter,
            // but this would make the usability of apiCallers very unpleasant.)
            // We could do this easily with https://github.com/Microsoft/TypeScript/issues/5453,
            // but changing the implementation would be a significant breaking change by then.
            return resp
        } catch (thrown) {
            if (axios.isCancel(thrown)) {
                // No handling of anything for cancellations.
                // A cancellation is deliberate and shouldn't be treated as an error state. Callbacks should not be called either - pretend the request never happened.
                // If a compelling case for invoking callbacks on cancel is found,
                // it should probably be implemented as a separate set of callbacks.
                // We don't set isLoading to false here - we set it in the cancel() method to ensure that we don't set isLoading=false for a subsequent call,
                // since the promise won't reject immediately after requesting cancelation. There could already be another request pending when this code is being executed.
                return;
            } else {
                var error = thrown as AxiosError;
            }

            delete this._cancelToken
            this.wasSuccessful = false
            const result = error.response as AxiosResponse<ListResult<TResult> | ItemResult<TResult>> | undefined
            if (result && typeof result.data === "object") {
                this.setResponseProps(result.data)
            } else {
                this.message = 
                    typeof error.message === "string" ? error.message : 
                    typeof error === "string" ? error :
                    "A network error occurred" // TODO: i18n
            }

            this._callbacks.onRejected.forEach(cb => cb.apply(thisArg, [this]))

            this.isLoading = false

            return error
        } finally {
            (this.apiClient as any)._nextCancelToken = null

            if (this._debounceSignal) {
                this._debounceSignal.resolve()
                this._debounceSignal = null;
            }
        }
    }

    protected _makeReactive() {
        // Make properties reactive. Works around https://github.com/vuejs/vue/issues/6648 

        // Use Object.keys to mock the behavior of 
        // https://github.com/vuejs/vue/blob/4c7a87e2ef9c76b5b75d85102662a27165a23ec7/src/core/observer/index.js#L61
        const keys = Object.keys(this)
        for (let i = 0; i < keys.length; i++) {
            const key = keys[i];

            // @ts-ignore - Ignore indexer on type without indexer signature.
            const value = this[key] 

            // Don't define sealed object properties (e.g. this._callbacks)
            if (value == null || typeof value !== "object" || !Object.isSealed(value)) {
                Vue.util.defineReactive(this, key, value)
            }
        }
    }

    constructor(
        private readonly apiClient: TClient,
        private readonly invoker: TInvoker<TArgs, ApiResultPromise<TResult>, TClient>
    ) { 
        super();

        // Create our invoker function that will ultimately be our instance object.
        const invokeFunc: TCall<TArgs, ApiResultPromise<TResult>> = function invokeFunc(this: any, ...args: TArgs) {
            return invoke._invokeInternal(this, ...args);
        } as TCall<TArgs, ApiResultPromise<TResult>>

        // Copy all properties from the class to the function.
        const invoke = Object.assign(invokeFunc, this);
        invoke.invoke = invoke;

        Object.setPrototypeOf(invoke, new.target.prototype);
        return invoke
    }
}

export class ItemApiState<TArgs extends any[], TResult, TClient extends ApiClient<any>> extends ApiState<TArgs, TResult, TClient> {

    /** The metadata of the method being called, if it was provided. */
    metadata?: ItemMethod

    /** Validation issues returned by the previous request. */
    validationIssues: ValidationIssue[] | null = null

    /** Principal data returned by the previous request. */
    result: TResult | null = null

    constructor(
        apiClient: TClient,
        invoker: TInvoker<TArgs, ApiResultPromise<TResult>, TClient>
    ) {
        super(apiClient, invoker);
        this._makeReactive();
    }

    protected setResponseProps(data: ItemResult<TResult>) {
        this.wasSuccessful = data.wasSuccessful
        this.message = data.message || null

        if ("validationIssues" in data) {
            this.validationIssues = data.validationIssues || null;
        } else {
            this.validationIssues = null;
        }
        if ("object" in data) {
            this.result = data.object || null
        } else {
            this.result = null
        }
    }
}

export class ListApiState<TArgs extends any[], TResult, TClient extends ApiClient<any>> extends ApiState<TArgs, TResult, TClient> {

    /** The metadata of the method being called, if it was provided. */
    metadata?: ListMethod

    /** Page number returned by the previous request. */
    page: number | null = null
    /** Page size returned by the previous request. */
    pageSize: number | null = null
    /** Page count returned by the previous request. */
    pageCount: number | null = null
    /** Total Count returned by the previous request. */
    totalCount: number | null = null

    /** Principal data returned by the previous request. */
    result: TResult[] | null = null

    constructor(
        apiClient: TClient,
        invoker: TInvoker<TArgs, ApiResultPromise<TResult>, TClient>
    ) {
        super(apiClient, invoker);
        this._makeReactive();
    }

    protected setResponseProps(data: ListResult<TResult>) {
        this.wasSuccessful = data.wasSuccessful
        this.message = data.message || null

        this.page = data.page
        this.pageSize = data.pageSize
        this.pageCount = data.pageCount
        this.totalCount = data.totalCount

        if ("list" in data) {
            this.result = data.list || []
        } else {
            this.result = null
        }
    }
}
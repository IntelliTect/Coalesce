
// Undocumented (but exposed) vue method for making properties reactive.
declare module "vue/types/vue" {
    interface VueConstructor<V extends Vue = Vue> {
        util: {
            defineReactive: (obj: any, key: string, val: any, setter: Function | null, shallow: boolean) => void
        }
    }
}

import { IHaveMetadata, ModelType, ClassType } from './metadata'
import { Model, hydrateModel, mapToDto } from './model'
import { OwnProps } from './util'

import axios, { AxiosPromise, AxiosResponse, AxiosError, AxiosRequestConfig, Canceler, CancelTokenSource } from 'axios'
import * as qs from 'qs'
import Vue from 'vue';


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
    includes?: string
    dataSource?: never //Idatasource
}
export interface FilterParameters extends DataSourceParameters {
    search?: string
    filter?: { [fieldName: string]: string }
}
export interface ListParameters extends FilterParameters {
    page?: number
    pageSize?: number
    orderBy?: string
    orderByDescending?: string
    fields?: string[]
}

export type ApiResponse<T> = Promise<AxiosResponse<T>>
export type AxiosItemResult<T> = AxiosResponse<ItemResult<T>>
export type AxiosListResult<T> = AxiosResponse<ListResult<T>>
export type ItemResultPromise<T> = Promise<AxiosResponse<ItemResult<T>>>
export type ListResultPromise<T> = Promise<AxiosResponse<ListResult<T>>>
// ApiResultPromise must be the inner union of the promise types, not the outer union of two promises.
// Otherwise, typescript doesn't like us calling a function with a union return type. For some reason.
export type ApiResultPromise<T> = Promise<AxiosItemResult<T> | AxiosListResult<T>>

// TODO: temp config for development
/** Axios instance to be used by all Coalesce API requests. Can be configured as needed. */
export const AxiosClient = axios.create()
AxiosClient.defaults.baseURL = 'http://localhost:11202/api/'
AxiosClient.defaults.withCredentials = true

export class ApiClient<T extends Model<ClassType>> {
    
    constructor(public $metadata: ModelType) {
        
    }

    /** Injects a cancellation token into the next request. */
    private _nextCancelToken: CancelTokenSource | null = null


    // TODO: should the standard set of endpoints be prefixed with $ 

    public get(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .get(
                `/${this.$metadata.controllerRoute}/get/${id}`, 
                this.$options(parameters, config)
            )
            .then<AxiosItemResult<T>>(this.$hydrateItemResult.bind(this))
    }
    
    public list(parameters?: ListParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .get(
                `/${this.$metadata.controllerRoute}/list`, 
                this.$options(parameters, config)
            )
            .then<AxiosListResult<T>>(this.$hydrateListResult.bind(this))
    }
    
    public count(parameters?: FilterParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .get<AxiosItemResult<number>>(
                `/${this.$metadata.controllerRoute}/count`, 
                this.$options(parameters, config)
            )
    }
    
    public save(item: T, parameters?: DataSourceParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .post(
                `/${this.$metadata.controllerRoute}/save`,
                qs.stringify(mapToDto(item)),
                this.$options(parameters, config)
            )
            .then<AxiosItemResult<T>>(this.$hydrateItemResult.bind(this))
    }
    
    public delete(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .post(
                `/${this.$metadata.controllerRoute}/delete/${id}`,
                null,
                this.$options(parameters, config)
            )
            .then<AxiosItemResult<T>>(this.$hydrateItemResult.bind(this))
    }

    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "item" indicating that the API endpoint returns an ItemResult<T>
     * @param invokerFactory method that will return a function that can be used to call the API. The signature of the returned function will be the call signature of the wrapper.
     */
    $caller<TCall extends (this: null, ...args: any[]) => ItemResultPromise<T>>(
        resultType: "item",
        invokerFactory: (client: this) => TCall
    ): ItemApiState<TCall, T> & TCall
    /**
     * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
     * @param resultType "list" indicating that the API endpoint returns an ListResult<T>
     * @param invokerFactory method that will return a function that can be used to call the API. The signature of the returned function will be the call signature of the wrapper.
     */
    $caller<TCall extends (this: null, ...args: any[]) => ListResultPromise<T>>(
        resultType: "list",
        invokerFactory: (client: this) => TCall
    ): ListApiState<TCall, T> & TCall
    
    $caller<TCall extends (this: null, ...args: any[]) => Promise<AxiosResponse<ApiResult>>>(
        resultType: "item" | "list", // TODO: Eventually this should be replaced with a metadata object I think
        invokerFactory: (client: this) => TCall
    ): ApiState<TCall, T> & TCall
    {
        var instance: ApiState<TCall, T>;
        switch (resultType){
            case "item": 
                instance = new ItemApiState<TCall, T>(this, invokerFactory(this));
                break;
            // Typescript is unhappy with giving TCall to ListApiState. No idea why, since the item one is fine.
            case "list": 
                instance = new ListApiState<any, T>(this, invokerFactory(this));
                break;
            default: throw `Unknown result type ${resultType}`
        }
        
        return instance as any;
    }

    private $options(parameters?: ListParameters | FilterParameters | DataSourceParameters, config?: AxiosRequestConfig) {
        // Merge standard Coalesce params with general configured params if there are any.
        var mergedParams = config && config.params 
            ? Object.assign({}, config && config.params, this.$objectify(parameters))
            : this.$objectify(parameters)

        // Params come last to overwrite config.params with our merged params object.
        return Object.assign({}, 
            { cancelToken: this._nextCancelToken && this._nextCancelToken.token }, 
            config, 
            { params: mergedParams }
        )
    }

    private $objectify(parameters?: ListParameters | FilterParameters | DataSourceParameters) {
        if (!parameters) return null

        // This implementation is fairly naive - it will map out ANYTHING that comes in.
        // We may want to move to only mapping known good parameters instead.
        var paramsObject = Object.assign({}, parameters)

        // Remove complex properties and replace them with their transport-mapped key-value-pairs.
        // This is probably only dataSource
        if (paramsObject.dataSource) {
            throw ("data source not supported yet")
        }
        return paramsObject
    }

    private $hydrateItemResult(value: AxiosItemResult<T>) {
        // This function is NOT PURE - we mutate the result object on the response.
        const object = value.data.object
        if (object) {
            hydrateModel(object, this.$metadata)
        }
        return value
    }

    private $hydrateListResult(value: AxiosListResult<T>) {
        // This function is NOT PURE - we mutate the result object on the response.
        const list = value.data.list
        if (Array.isArray(list)) {
            list.forEach(item => hydrateModel(item, this.$metadata))
        }
        return value
    }
}

abstract class ApiState<TCall extends (this: null, ...args: any[]) => ApiResultPromise<T>, T extends Model<ClassType>> extends Function {

    /** True if a request is currently pending. */
    isLoading: boolean = false
    
    /** True if the previous request was successful. */
    wasSuccessful: boolean | null = null
    
    /** Error message returned by the previous request. */
    message: string | null = null

    /** 
     * Function that can be called to cancel a pending request.
    */
    cancel: Canceler | null = null

    // Frozen to prevent unneeded reactivity.
    private _callbacks = Object.freeze<{
        onFulfilled: Array<Function>, 
        onRejected: Array<Function>
    }>({onFulfilled: [], onRejected: []})

    /**
     * Attach a callback to be invoked when the request to this endpoint succeeds.
     * @param onFulfilled A callback to be called when a request to this endpoint succeeds.
     */
    onFulfilled(callback: (state: this) => void): this {
        this._callbacks.onFulfilled.push(callback)
        return this;
    }

    /**
     * Attach a callback to be invoked when the request to this endpoint fails.
     * @param onFulfilled A callback to be called when a request to this endpoint fails.
     */
    onRejected(callback: (state: this) => void): this {
        this._callbacks.onFulfilled.push(callback)
        return this;
    }

    abstract setResponseProps(data: ApiResult): void

    invoke!: TCall

    _invokeInternal() {
        if (this.isLoading) {
            throw `Request is already pending for invoker ${this.invoker.toString()}`
        }
        this.wasSuccessful = null
        this.message = null
        this.isLoading = true

        // Inject a cancellation token into the request.
        var promise: ApiResultPromise<T>
        try {
            const token = (this.apiClient as any)._nextCancelToken = axios.CancelToken.source()
            this.cancel = token.cancel
            promise = this.invoker.apply(null, arguments)
        } finally {
            (this.apiClient as any)._nextCancelToken = null
        }

        return promise
            .then(resp => {
                const data = resp.data
                this.cancel = null
                this.setResponseProps(data)

                this._callbacks.onFulfilled.forEach(cb => cb.apply(this, [this]))

                this.isLoading = false

                // We have to maintain the shape of the promise of the stateless invoke method.
                // This means we can't re-shape ourselves into a Promise<ApiState<T>> with `return fn` here.
                // The reason for this is that we can't change the return type of TCall while maintaining 
                // the param signature (unless we required a full, explicit type annotation as a type parameter,
                // but this would make the usability of apiCallers very unpleasant.)
                // We could do this easily with https://github.com/Microsoft/TypeScript/issues/5453,
                // but changing the implementation would be a significant breaking change by then.
                return resp
            }, (error: AxiosError) => {
                this.cancel = null
                this.wasSuccessful = false
                const result = error.response as AxiosResponse<ListResult<T> | ItemResult<T>> | undefined
                if (result) {
                    const data = result.data
                    this.setResponseProps(data)
                } else {
                    // TODO: i18n
                    this.message = error.message || "A network error occurred"
                }

                this._callbacks.onRejected.forEach(cb => cb.apply(this, [this]))

                this.isLoading = false

                return error
            })
    }

    constructor(
        private readonly apiClient: ApiClient<T>,
        private readonly invoker: TCall
    ) { 
        super();
        const self = this;
        // Create our invoker function that will ultimately be our instance object.
        const invokeFunc: TCall = function invokeFunc() {
            return invoke._invokeInternal.apply(invoke, arguments);
        } as TCall
        // Copy all properties from the class to the function.
        const invoke = Object.assign(invokeFunc, this);
        invoke.invoke = invoke;
        
        // Make properties reactive. Works around https://github.com/vuejs/vue/issues/6648 
        for (const stateProp in self) {
            const value = self[stateProp]
            // Don't define sealed object properties (e.g. this._callbacks)
            if (value != null && typeof value !== "object" || !Object.isSealed(value)) {
                Vue.util.defineReactive(invoke, stateProp, self[stateProp], null, true)
            }
        }

        Object.setPrototypeOf(invoke, new.target.prototype);
        return invoke
    }
}

export class ItemApiState<TCall extends (this: null, ...args: any[]) => ItemResultPromise<T>, T extends Model<ClassType>> extends ApiState<TCall, T> {
    /** Validation issues returned by the previous request. */
    validationIssues: ValidationIssue[] | null = null

    /** Principal data returned by the previous request. */
    result: T | null = null

    setResponseProps(data: ItemResult<T>) {
        this.wasSuccessful = data.wasSuccessful
        this.message = data.message || null

        if ("object" in data) {
            this.result = data.object || null
        } else {
            this.result = null
        }
    }
}

export class ListApiState<TCall extends (this: null, ...args: any[]) => ListResultPromise<T>, T extends Model<ClassType>> extends ApiState<TCall, T> {
    /** Page number returned by the previous request. */
    page: number | null = null
    /** Page size returned by the previous request. */
    pageSize: number | null = null
    /** Page count returned by the previous request. */
    pageCount: number | null = null
    /** Total Count returned by the previous request. */
    totalCount: number | null = null

    /** Principal data returned by the previous request. */
    result: T[] | null = null

    setResponseProps(data: ListResult<T>) {
        this.wasSuccessful = data.wasSuccessful
        this.message = data.message || null

        if ("list" in data) {
            this.result = data.list || []
        } else {
            this.result = null
        }
    }
}
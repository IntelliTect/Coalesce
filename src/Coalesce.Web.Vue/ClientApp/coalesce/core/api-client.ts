
import { IHaveMetadata, ModelType, hydrateMetadata, ClassType } from './metadata'

import axios, { AxiosPromise, AxiosResponse, AxiosError, AxiosRequestConfig, Canceler, CancelTokenSource } from 'axios';
import * as qs from 'qs';
import { mapToDto } from '../index';
import { Model } from './model';
import { OwnProps } from './util';




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
    dataSource?: never; //Idatasource;
}
export interface FilterParameters extends DataSourceParameters {
    search?: string;
    filter?: { [fieldName: string]: string };
}
export interface ListParameters extends FilterParameters {
    page?: number;
    pageSize?: number;
    orderBy?: string;
    orderByDescending?: string;
    fields?: string[];
}

        
interface EndpointState<T> {
    loading: boolean;
    wasSuccessful: boolean | null;
    message: string | null;
    result: T | null;
    cancel: Canceler | null;
}
interface ItemEndpointState<T> extends EndpointState<T> {
    validationIssues: ValidationIssue[] | null;
}

interface ListEndpointState<T> extends EndpointState<Array<T>> {
    page: number | null;
    pageSize: number | null;
    pageCount: number | null;
    totalCount: number | null;
}

type ApiResponse<T> = Promise<AxiosResponse<T>>

type AxiosItemResult<T> = AxiosResponse<ItemResult<T>>
type AxiosListResult<T> = AxiosResponse<ListResult<T>>
type ItemResultPromise<T> = Promise<AxiosItemResult<T>>;
type ListResultPromise<T> = Promise<AxiosListResult<T>>;
// ApiResultPromise must be the inner union of the promise types, not the outer union of two promises.
// Otherwise, typescript doesn't like us calling a function with a union return type. For some reason.
type ApiResultPromise<T> = Promise<AxiosItemResult<T> | AxiosListResult<T>>
type ItemCaller<TCall, U> = (TCall) & ItemEndpointState<U>;
type ListCaller<TCall, U> = (TCall) & ListEndpointState<U>;
type Caller<TCall, U> = ItemCaller<TCall, U> | ListCaller<TCall, U>

// TODO: temp config for development
export const AxiosClient = axios.create();
AxiosClient.defaults.baseURL = 'http://localhost:11202/api/';
AxiosClient.defaults.withCredentials = true;


export class ApiClient<T extends Model<ClassType>> {
    
    constructor(public readonly metadata: ModelType) {
        
    }

    private nextCancelToken: CancelTokenSource | null = null;

    public get(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .get(
                `/${this.metadata.controllerRoute}/get/${id}`, 
                this.options(parameters, config)
            )
            .then<AxiosItemResult<T>>(this.hydrateItemResult.bind(this))
    }
    
    public list(parameters?: ListParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .get(
                `/${this.metadata.controllerRoute}/list`, 
                this.options(parameters, config)
            )
            .then<AxiosListResult<T>>(this.hydrateListResult.bind(this))
    }
    
    public count(parameters?: FilterParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .get<AxiosItemResult<number>>(
                `/${this.metadata.controllerRoute}/count`, 
                this.options(parameters, config)
            )
    }
    
    public save(item: T, parameters?: DataSourceParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .post(
                `/${this.metadata.controllerRoute}/save`,
                qs.stringify(mapToDto(item)),
                this.options(parameters, config)
            )
            .then<AxiosItemResult<T>>(this.hydrateItemResult.bind(this))
    }
    
    public delete(id: string | number, parameters?: DataSourceParameters, config?: AxiosRequestConfig) {
        return AxiosClient
            .post(
                `/${this.metadata.controllerRoute}/delete/${id}`,
                null,
                this.options(parameters, config)
            )
            .then<AxiosItemResult<T>>(this.hydrateItemResult.bind(this))
    }

    makeApiCaller<TCall extends (this: null, ...args: any[]) => ItemResultPromise<T>>(
        resultType: "item",
        invokerFactory: (client: this) => TCall
    ): TCall & EndpointState<T>
    makeApiCaller<TCall extends (this: null, ...args: any[]) => ItemResultPromise<T>, TResult = T>(
        resultType: "item",
        invokerFactory: (client: this) => TCall, 
        handle: (promise: Promise<ItemEndpointState<T>>) => TResult | PromiseLike<TResult>
    ): TCall & EndpointState<T>
    makeApiCaller<TCall extends (this: null, ...args: any[]) => ListResultPromise<T>, TResult = T>(
        resultType: "list",
        invokerFactory: (client: this) => TCall
    ): TCall & ListEndpointState<T>
    makeApiCaller<TCall extends (this: null, ...args: any[]) => ListResultPromise<T>, TResult = T>(
        resultType: "list",
        invokerFactory: (client: this) => TCall, 
        handle: (promise: Promise<ListEndpointState<T>>) => ListResultPromise<TResult>
    ): TCall & ListEndpointState<T>
    makeApiCaller<TCall extends (this: null, ...args: any[]) => ApiResultPromise<T>, TResult = T>(
        resultType: "item" | "list", // TODO: Eventually this should be replaced with a metadata object I think
        invokerFactory: (client: this) => TCall, 
        handle?: (...args: any[]) => any
    ): TCall & (ItemEndpointState<T> | ListEndpointState<T>)
    {
        var fn: Caller<TCall, T>
        
        const _this = this;
        const invoker = invokerFactory(_this);
        // Can't use arrow function, need access to 'arguments'.
        function invoke() {
            fn.wasSuccessful = null
            fn.message = null
            fn.loading = true

            // Inject a cancellation token into the request.
            var promise: ApiResultPromise<T>;
            try {
                const token = _this.nextCancelToken = axios.CancelToken.source()
                fn.cancel = token.cancel
                promise = invoker.apply(null, arguments)
            } finally {
                _this.nextCancelToken = null
            }

            function setResponseProps(data: ItemResult<T> | ListResult<T>) {
                fn.wasSuccessful = data.wasSuccessful
                fn.message = data.message || null

                if ("validationIssues" in fn) {
                    fn.validationIssues = "validationIssues" in data && data.validationIssues || null;
                }

                if ("list" in data) {
                    fn.result = data.list || []
                } else if ("object" in data) {
                    fn.result = data.object || null
                } else {
                    fn.result = null
                }
            }

            const stateMappedPromise = promise
                .then(resp => {
                    const data = resp.data
                    fn.cancel = null
                    setResponseProps(data);

                    fn.loading = false
                    return fn
                }, (error: AxiosError) => {
                    fn.cancel = null
                    fn.wasSuccessful = false
                    const result = error.response as AxiosResponse<ListResult<T> | ItemResult<T>> | undefined
                    if (result) {
                        const data = result.data;
                        setResponseProps(data);
                    } else {
                        // TODO: i18n
                        fn.message = error.message || "A network error occurred"
                    }
                    fn.loading = false
                    return fn;
                })

            if (handle) {
                return handle(stateMappedPromise);
            } else {
                return stateMappedPromise;
            }
        }

        const stateProps = Object.assign({
            loading: false,
            wasSuccessful: null,
            message: null,
            result: null,
            cancel: null,
        } as EndpointState<T>, resultType == "list" 
            ? {
                page: null,
                pageSize: null,
                pageCount: null,
                totalCount: null,
            } as OwnProps<ListEndpointState<T>, EndpointState<T>> 
            : {
                validationIssues: null
            } as OwnProps<ItemEndpointState<T>, EndpointState<T>>
        ) as ItemEndpointState<T> | ListEndpointState<T>

        return fn = Object.assign(
            invoke as TCall, 
            stateProps
        )
    }

    private options(parameters?: ListParameters | FilterParameters | DataSourceParameters, config?: AxiosRequestConfig) {
        // Merge standard Coalesce params with general configured params if there are any.
        var mergedParams = config && config.params 
            ? Object.assign({}, config && config.params, this.objectify(parameters))
            : this.objectify(parameters);

        // Params come last to overwrite config.params with our merged params object.
        return Object.assign({}, 
            { cancelToken: this.nextCancelToken && this.nextCancelToken.token }, 
            config, 
            { params: mergedParams }
        );
    }

    private objectify(parameters?: ListParameters | FilterParameters | DataSourceParameters) {
        if (!parameters) return null;

        // This implementation is fairly naive - it will map out ANYTHING that comes in.
        // We may want to move to only mapping known good parameters instead.
        var paramsObject = Object.assign({}, parameters);

        // Remove complex properties and replace them with their transport-mapped key-value-pairs.
        // This is probably only dataSource
        if (paramsObject.dataSource) {
            throw ("data source not supported yet")
        }
        return paramsObject;
    }

    private hydrateItemResult(value: AxiosItemResult<T>) {
        // This function is NOT PURE - we mutate the result object on the response.
        const object = value.data.object;
        if (object) {
            hydrateMetadata(object, this.metadata)
        }
        return value;
    }

    private hydrateListResult(value: AxiosListResult<T>) {
        // This function is NOT PURE - we mutate the result object on the response.
        const list = value.data.list;
        if (Array.isArray(list)) {
            list.forEach(item => hydrateMetadata(item, this.metadata))
        }
        return value;
    }
}
import Vue from "vue";

// Undocumented (but exposed) vue method for making properties reactive.
declare module "vue/types/vue" {
  interface VueConstructor {
    util: {
      defineReactive: (
        obj: any,
        key: string,
        val: any,
        setter?: Function | null,
        shallow?: boolean
      ) => void;
    };
  }
}

import {
  ModelType,
  ClassType,
  Method,
  Service,
  ApiRoutedType,
  DataSourceType,
  Value,
  ModelValue,
  CollectionValue,
  VoidValue,
  ItemMethod,
  ListMethod,
  TypeDiscriminatorToType,
  PropNames
} from "./metadata";
import {
  Model,
  convertToModel,
  mapToDto,
  DataSource,
  mapToModel,
  mapToDtoFiltered,
  parseValue
} from "./model";
import { OwnProps, Indexable, objectToQueryString } from "./util";

import axios, {
  AxiosPromise,
  AxiosResponse,
  AxiosError,
  AxiosRequestConfig,
  Canceler,
  CancelTokenSource,
  CancelToken,
  AxiosInstance,
  Cancel
} from "axios";

/* Api Response Objects */

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

/* Api Parameter Objects */

export interface DataSourceParameters {
  /** A string that the server may use to include/exclude certain data from the results. See Coalesce's full documentation for details. */
  includes?: string | null;

  /**
   * A data source instance that will be used to load the data.
   * Classes are found in `models.g.ts` as `<ModelName>.DataSources.<DataSourceName>`, e.g. `Person.DataSources.WithRelations`.
   */
  dataSource?: DataSource<DataSourceType> | null;
}
export class DataSourceParameters {
  constructor() {
    this.includes = null;
    this.dataSource = null;
  }
}

export interface SaveParameters<T extends Model<ModelType> = any> extends DataSourceParameters {
  /**
   * A list of field names to save. 
   * If set, only the specified fields as well as any primary key
   * will be sent to the server. The server will ignore fields that are not set.
   */
  fields?: PropNames<T["$metadata"]>[] | null;
}
export class SaveParameters<T extends Model<ModelType>> extends DataSourceParameters {
  constructor() {
    super();
    this.fields = null;
  }
}

export interface FilterParameters extends DataSourceParameters {
  /** A search term to search by. Searching behavior is determined by the server. */
  search?: string | null;

  /** A collection of key-value pairs to filter by. Behavior is dependent on the type of each field, see Coalesce's full documentation for details. */
  filter?: { [fieldName: string]: string | number | boolean | null } | null;
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
export class ListParameters extends FilterParameters {
  constructor() {
    super();
    this.page = 1;
    this.pageSize = 10;
    this.orderBy = null;
    this.orderByDescending = null;
    this.fields = null;
  }
}

/**
 * Maps the given API parameters object into an flat object of key-value pairs
 * that is suitable for use as a URL querystring.
 * @param parameters The parameters to map.
 */
export function mapParamsToDto(
  parameters?: ListParameters | FilterParameters | DataSourceParameters | SaveParameters
) {
  if (!parameters) return null;

  // Assume the widest type, which is ListParameters.
  var wideParams = parameters as Partial<ListParameters>;

  // The list of 'simple' params where we just pass along the exact value.
  var simpleParams = [
    "includes",
    "search",
    "page",
    "pageSize",
    "orderBy",
    "orderByDescending"
  ] as const;

  // Map all the simple params to `paramsObject`
  var paramsObject = simpleParams.reduce(
    (obj, key) => {
      if (key in wideParams && wideParams[key] != null) {
        obj[key] = String(wideParams[key]);
      }
      return obj;
    },
    {} as { [s: string]: string }
  );

  // Map the 'filter' object, ensuring all values are strings.
  const filter = wideParams.filter;
  if (typeof filter == "object" && filter) {
    for (var key in filter) {
      let value = filter[key];
      if (value !== undefined) {
        paramsObject["filter." + key] = String(value);
      }
    }
  }

  if (Array.isArray(wideParams.fields)) {
    paramsObject.fields = wideParams.fields.join(",");
  }

  // Map the data source and its params
  const dataSource = wideParams.dataSource as Indexable<
    typeof wideParams.dataSource
  >;
  if (dataSource) {
    // Add the data source name
    paramsObject["dataSource"] = dataSource.$metadata.name;
    var paramsMeta = dataSource.$metadata.props;

    // Add the data source parameters.
    // Note that we use "dataSource.{paramName}", not a nested object.
    // This is what the model binder expects.
    for (var paramName in paramsMeta) {
      const paramMeta = paramsMeta[paramName];
      if (paramName in dataSource) {
        const paramValue = dataSource[paramName];
        paramsObject["dataSource." + paramMeta.name] = mapToDto(
          paramValue,
          paramMeta
        ) as any;
      }
    }
  }

  return paramsObject;
}


/**
 * Maps the given flat object of key-value pairs into an API parameters object.
 * @param dto The flat object to map.
 * @param parametersType The constructor of the parameters object to create.
 * @param modelClass The generated model class (containing a `DataSources` namespace) 1
 */
export function mapQueryToParams<T extends DataSourceParameters>(
  flatQuery: any,
  parametersType: new() => T,
  modelMeta: ModelType,
): T {
  const dto = flatQuery; // alias for brevity
  
  const parameters: DataSourceParameters | FilterParameters | ListParameters
    = new parametersType;

  if (parameters instanceof ListParameters) {
    if ('page' in dto) parameters.page = +dto.page;
    if ('pageSize' in dto) parameters.pageSize = +dto.pageSize;
    if ('orderBy' in dto) parameters.orderBy = dto.orderBy;
    if ('orderByDescending' in dto) parameters.orderByDescending = dto.orderByDescending;
    if ('fields' in dto) parameters.fields = String(dto.fields).split(",")
  }

  if (parameters instanceof FilterParameters) {
    if ('search' in dto) parameters.search = dto.search;
    for (const key in dto) {
      if (key.startsWith("filter.") && dto[key] !== undefined) {
        parameters.filter = parameters.filter ?? {}
        parameters.filter[key.replace("filter.", "")] = dto[key]
      }
    }
  }

  if (parameters instanceof DataSourceParameters) {
    if ('includes' in dto) parameters.includes = dto.includes;

    if ('dataSource' in dto && dto.dataSource in modelMeta.dataSources) {
      const dataSource = mapToModel({}, modelMeta.dataSources[dto.dataSource])
      
      for (const key in dto) {
        if (key.startsWith("dataSource.")) {
          var paramName = key.replace("dataSource.", "");
          if (paramName in dataSource.$metadata.props) {
            (dataSource as any)[paramName] = mapToModel(dto[key], dataSource.$metadata.props[paramName])
          }
        }
      }
    }
  }

  return parameters as T;
}



export function getMessageForError(error: AxiosError | ApiResult | Error | string) {
  if (typeof error === "string") return error;

  if ("response" in error) {
    const result = error.response as
      | AxiosResponse<ListResult<any> | ItemResult<any>>
      | undefined;

    if (result && typeof result.data === "object") {
      return result.data.message || "Unknown Error"
    }
  }

  return typeof error.message === "string"
    ? error.message
    : "A network error occurred"; // TODO: i18n
}

export type AxiosItemResult<T> = AxiosResponse<ItemResult<T>>;
export type AxiosListResult<T> = AxiosResponse<ListResult<T>>;
export type ItemResultPromise<T> = Promise<AxiosResponse<ItemResult<T>>>;
export type ListResultPromise<T> = Promise<AxiosResponse<ListResult<T>>>;
// ApiResultPromise must be the inner union of the promise types, not the outer union of two promises.
// Otherwise, typescript doesn't like us calling a function with a union return type. For some reason.
export type ApiResultPromise<T> = Promise<
  AxiosItemResult<T> | AxiosListResult<T>
>;

/** Axios instance to be used by all Coalesce API requests. Can be configured as needed. */
export const AxiosClient = axios.create();
AxiosClient.defaults.baseURL = "/api";

export type ItemApiReturnType<
  T extends (this: null, ...args: any[]) => ItemResultPromise<any>
> = ReturnType<T> extends void
  ? void
  : ReturnType<T> extends ItemResultPromise<infer R>
  ? R
  : any;

export type ListApiReturnType<
  T extends (this: null, ...args: any[]) => ListResultPromise<any>
> = ReturnType<T> extends ListResultPromise<infer S> ? S : any;

type AnyApiReturnType<
  T extends (this: null, ...args: any[]) => ApiResultPromise<any>
> = ReturnType<T> extends ApiResultPromise<infer S> ? S : any;

export type ApiCallerConcurrency = "cancel" | "disallow" | "allow" | "debounce";

type ItemTransportTypeSpecifier<T extends ApiRoutedType> =
  | "item"
  | ItemMethod
  | ((methods: T["methods"]) => ItemMethod);

type ListTransportTypeSpecifier<T extends ApiRoutedType> =
  | "list"
  | ListMethod
  | ((methods: T["methods"]) => ListMethod);

type TransportTypeSpecifier<T extends ApiRoutedType> =
  | ItemTransportTypeSpecifier<T>
  | ListTransportTypeSpecifier<T>;

type ResultPromiseType<
  T extends TransportTypeSpecifier<any>,
  TResult
> = T extends ItemTransportTypeSpecifier<any>
  ? ItemResultPromise<TResult>
  : T extends ListTransportTypeSpecifier<any>
  ? ListResultPromise<TResult>
  : never;


type InvokerReturnType<
  T extends TransportTypeSpecifier<any>,
  TResult
> = T extends ItemTransportTypeSpecifier<any>
  ? ItemResultPromise<TResult>
  : T extends ListTransportTypeSpecifier<any>
  ? ListResultPromise<TResult>
  : never;

type InvokerType<
  T extends TransportTypeSpecifier<any>,
  TArgs extends any[],
  TResult
> = TCall<TArgs, InvokerReturnType<T, TResult>>;

type ApiStateType<
  T extends TransportTypeSpecifier<any>,
  TArgs extends any[],
  TResult
> = (T extends ItemTransportTypeSpecifier<any>
  ? ItemApiState<TArgs, TResult>
  : T extends ListTransportTypeSpecifier<any>
  ? ListApiState<TArgs, TResult>
  : never) & InvokerType<T, TArgs, TResult>;

type ApiStateTypeWithArgs<
  T extends TransportTypeSpecifier<any>,
  TArgs extends any[],
  TArgsObj,
  TResult
> = (T extends ItemTransportTypeSpecifier<any>
  ? ItemApiStateWithArgs<TArgs, TArgsObj, TResult>
  : T extends ListTransportTypeSpecifier<any>
  ? ListApiStateWithArgs<TArgs, TArgsObj, TResult>
  : never) & InvokerType<T, TArgs, TResult>;


const simultaneousGetCache: Map<string, AxiosPromise<any>> = new Map;


export class ApiClient<T extends ApiRoutedType> {
  constructor(public $metadata: T) {}

  /** Cancellation token to inject into the next request. */
  private _nextCancelToken?: CancelTokenSource;

  /** Flag to enable global caching of identical GET requests
   * that have been made simultaneously.
   */
  protected _simultaneousGetCaching = false;

  /** Enable simultaneous request caching, causing identical GET requests made 
   * at the same time across all ApiClient instances to be handled with the same AJAX request.
   */
  public $withSimultaneousRequestCaching(): this {
    this._simultaneousGetCaching = true;
    return this;
  }

  /**
   * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
   * @param resultType An indicator of whether the API endpoint returns an ItemResult<T> or a ListResult<T>
   * @param invokerFactory method that will call the API. The signature of the function, minus the apiClient parameter, will be the call signature of the wrapper.
   */
  $makeCaller<
    TArgs extends any[],
    TResult,
    TTransportType extends TransportTypeSpecifier<T>
  >(
    resultType: TTransportType,
    invoker: TInvoker<TArgs, ResultPromiseType<TTransportType, TResult>, this>
  ): ApiStateType<TTransportType, TArgs, TResult>;

  /**
   * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
   * @param resultType An indicator of whether the API endpoint returns an ItemResult<T> or a ListResult<T>
   * @param invokerFactory method that will call the API. The signature of the function, minus the apiClient parameter, will be the call signature of the wrapper.
   */
  $makeCaller<
    TArgs extends any[],
    TResult,
    TTransportType extends TransportTypeSpecifier<T>
  >(
    resultType: TTransportType,
    invoker: TInvoker<
      TArgs,
      ResultPromiseType<TTransportType, TResult> | undefined | void,
      this
    >
  ): ApiStateType<TTransportType, TArgs, TResult | undefined>;

  /**
   * Create a wrapper function for an API call. This function maintains properties which represent the state of its previous invocation.
   * @param resultType An indicator of whether the API endpoint returns an ItemResult<T> or a ListResult<T>
   * @param invokerFactory method that will call the API. The signature of the function, minus the apiClient parameter, will be the call signature of the wrapper.
   * @param invokerFactory method that will call the API with an args object as the only parameter. This may be called by using `.withArgs()` on the function that is returned from `$makeCaller`. The value of the args object will default to `.args` if not specified.
   */
  $makeCaller<
    TArgs extends any[],
    TArgsObj extends {},
    TResult,
    TTransportType extends TransportTypeSpecifier<T>
  >(
    resultType: TTransportType,
    invoker: TInvoker<TArgs, ResultPromiseType<TTransportType, TResult>, this>,
    argsFactory?: () => TArgsObj,
    argsInvoker?: TArgsInvoker<
      TArgsObj,
      ResultPromiseType<TTransportType, TResult>,
      this
    >
  ): ApiStateTypeWithArgs<TTransportType, TArgs, TArgsObj, TResult>;


  $makeCaller<
    TArgs extends any[],
    TArgsObj extends {},
    TResult,
    TTransportType extends TransportTypeSpecifier<T>
  >(
    resultType: TTransportType,
    invoker: TInvoker<
      TArgs,
      ResultPromiseType<TTransportType, TResult> | undefined | void,
      this
    >,
    argsFactory?: () => TArgsObj,
    argsInvoker?: TArgsInvoker<
      TArgsObj,
      ResultPromiseType<TTransportType, TResult>,
      this
    >
  ): any {
    let localResultType: TransportTypeSpecifier<T> = resultType;
    let meta: Method | undefined = undefined;
    if (typeof localResultType === "function") {
      meta = localResultType(this.$metadata.methods);
      localResultType = meta.transportType;
    } else if (typeof localResultType === "object") {
      meta = localResultType;
      localResultType = localResultType.transportType;
    }

    // This is basically all just about resolving the overloads.
    // We use `any` because the types get really messy if you try to handle both types at once.

    var instance;
    if (argsFactory && argsInvoker) {
      switch (localResultType) {
        case "item":
          instance = new ItemApiStateWithArgs<TArgs, TArgsObj, TResult>(
            this,
            invoker as any,
            argsFactory,
            argsInvoker as any
          );
          break;
        case "list":
          instance = new ListApiStateWithArgs<TArgs, TArgsObj, TResult>(
            this,
            invoker as any,
            argsFactory,
            argsInvoker as any
          );
          break;
        default:
          throw `Unknown result type ${localResultType}`;
      }
    } else {
      switch (localResultType) {
        case "item":
          instance = new ItemApiState<TArgs, TResult>(
            this,
            invoker as any
          );
          break;
        case "list":
          instance = new ListApiState<TArgs, TResult>(
            this,
            invoker as any
          );
          break;
        default:
          throw `Unknown result type ${localResultType}`;
      }
    }

    // Set this on the instance if we have it.
    instance.$metadata = meta;

    return instance as any;
  }

  public $invoke<TMethod extends ItemMethod>(
    method: TMethod,
    params: ParamsObject<TMethod>,
    config?: AxiosRequestConfig
  ): AxiosPromise<ItemResult<any>>;

  public $invoke<TMethod extends ListMethod>(
    method: TMethod,
    params: ParamsObject<TMethod>,
    config?: AxiosRequestConfig
  ): AxiosPromise<ListResult<any>>;

  /**
   * Invoke the specified method using the provided set of parameters.
   * @param method The metadata of the API method to invoke
   * @param params The parameters to provide to the API method.
   * @param config A full `AxiosRequestConfig` to merge in.
   */
  public $invoke<TMethod extends Method>(
    method: TMethod,
    params: ParamsObject<TMethod>,
    config?: AxiosRequestConfig
  ) {
    const mappedParams = this.$mapParams(method, params);
    const url = `/${this.$metadata.controllerRoute}/${method.name}`;

    let body: any;
    let query: any;

    if (method.httpMethod != "GET" && method.httpMethod != "DELETE") {
      // The HTTP method has a body.

      query = undefined;
      if (Object.values(mappedParams).some(p => p instanceof Blob || p instanceof Uint8Array)) {
        // If the endpoint has any files or raw binary, we need to craft a FormData.
        const formData = body = new FormData;

        for (const key in mappedParams) {
          const value = mappedParams[key];
          if (value instanceof Blob) {
            // Add files normally.
            formData.append(key, value)
          } else if (value instanceof Uint8Array) {
            // Add raw binary as blobs
            formData.append(key, new Blob([value]))
          } else {
            // For non-files, stringify to get properly formatted key/value pairs
            // and then merge them into the formdata.
            // This is done because value could be a complex object that will result in 
            // lots of flattened key/value pairs
            const formPairs = objectToQueryString({[key]: value}).split("&");
            for (const pair of formPairs) {
              const [k,v] = pair.split("=")
              formData.append(decodeURIComponent(k), decodeURIComponent(v));
            }
          }
        }
      } else {
        // No top-level special values - just handle the params normally.
        body = objectToQueryString(mappedParams)
      }
    } else {
      // The HTTP method has no body.

      body = undefined;
      query = mappedParams
    }

    return this._possiblyCachedRequest(
      method.httpMethod,
      url,
      mappedParams,
      config,
      () => AxiosClient.request({
          method: method.httpMethod,
          url: url,
          data: body,
          ...this.$options(undefined, config, query)
        }).then(r => {
          switch (method.transportType) {
            case "item":
              return this.$hydrateItemResult(r, (method as ItemMethod).return);
            case "list":
              return this.$hydrateListResult(r, (method as ListMethod).return);
          }
        })
    );
  }

  /** Wraps a request, performing caching as needed according to _simultaneousGetCaching  */
  protected _possiblyCachedRequest(
    method: string,
    url: string, 
    parameters: any, 
    config: AxiosRequestConfig | undefined, 
    promiseFactory: () => AxiosPromise<any>
  ){
    let doCache = false;
    let cacheKey: string;
    
    if (method === "GET" && this._simultaneousGetCaching && !config) {
      cacheKey = url + "?" + objectToQueryString(parameters)
      if (simultaneousGetCache.has(cacheKey)) {
        return simultaneousGetCache.get(cacheKey)!
      } else {
        doCache = true;
      }
    }
      
    let promise = promiseFactory()

    if (doCache) {
      // Add the promise to the cache.
      simultaneousGetCache.set(cacheKey!, promise);

      // Remove the promise from the cache when it completes.
      promise = promise.then(x => {
        // Remove the request from the cache, because its done now.
        simultaneousGetCache.delete(cacheKey);
        return x;
      }, e => {
        simultaneousGetCache.delete(cacheKey);
        // Re-throw the error so callers down the line can handle it.
        throw e;
      })
    }

    return promise;
  }

  /**
   * Maps the given method parameters to values suitable for transport.
   * @param method The method whose parameters need mapping
   * @param params The values of the parameter to map
   */
  protected $mapParams(method: Method, params: { [paramName: string]: any }) {
    const formatted: { [paramName: string]: ReturnType<typeof mapToDto> | File | Blob | Uint8Array } = {};
    for (var paramName in method.params) {
      const paramMeta = method.params[paramName];
      const paramValue = params[paramName];

      if (paramMeta.type == "file" || paramMeta.type == "binary") {
        // Preserve top-level files and binary as their original format
        formatted[paramName] = parseValue(paramValue, paramMeta)
      } else {
        formatted[paramName] = mapToDto(paramValue, paramMeta);
      }
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
      cancelToken: this._nextCancelToken?.token ?? undefined,
      ...config,

      // Merge standard Coalesce params with general configured params if there are any.
      // Params come last to overwrite config.params with our merged params object.
      params: {
        ...queryParams,
        ...(config && config.params ? config.params : null),
        ...mapParamsToDto(parameters)
      }
    };
  }

  protected $hydrateItemResult<TResult>(
    value: AxiosItemResult<TResult>,
    metadata: Value | VoidValue
  ) {
    if (typeof value.data !== "object") {
      // This case usually only happens if the endpoint doesn't exist on the server,
      // causing the server to return a SPA fallback route (as HTML) with a 200 status.
      throw new Error(`Unexpected raw ${typeof value.data} response from server.`)
    }

    // Do nothing for void returns - there will be no object.
    if (metadata.type !== "void") {
      // This function is NOT PURE - we mutate the result object on the response.
      value.data.object = convertToModel(value.data.object, metadata);
    }
    return value;
  }

  protected $hydrateListResult<TResult>(
    value: AxiosListResult<TResult>,
    metadata: CollectionValue
  ) {
    if (typeof value.data !== "object") {
      // This case usually only happens if the endpoint doesn't exist on the server,
      // causing the server to return a SPA fallback route (as HTML) with a 200 status.
      throw new Error(`Unexpected raw ${typeof value.data} response from server.`)
    }

    // This function is NOT PURE - we mutate the result object on the response.
    value.data.list = convertToModel(value.data.list, metadata);
    return value;
  }
}

export type ParamsObject<TMethod extends Method> = {
  [P in keyof TMethod["params"]]: TypeDiscriminatorToType<
    TMethod["params"][P]["type"]
  > | null
};


export class ModelApiClient<TModel extends Model<ModelType>> extends ApiClient<
  TModel["$metadata"]
> {
  public get(
    id: string | number,
    parameters?: DataSourceParameters,
    config?: AxiosRequestConfig
  ): ItemResultPromise<TModel> {
    let url = `/${this.$metadata.controllerRoute}/get/${id}`;
    
    return this._possiblyCachedRequest(
      "GET",
      url,
      parameters,
      config,
      () => AxiosClient.get(
          url,
          this.$options(parameters, config)
        ).then<AxiosItemResult<TModel>>(r =>
          this.$hydrateItemResult(r, this.$itemValueMeta)
        )
    );
  }

  public list(
    parameters?: ListParameters,
    config?: AxiosRequestConfig
  ): ListResultPromise<TModel> {
    let url = `/${this.$metadata.controllerRoute}/list`;
    
    return this._possiblyCachedRequest(
      "GET",
      url,
      parameters,
      config,
      () => AxiosClient.get(
          url,
          this.$options(parameters, config)
        ).then<AxiosListResult<TModel>>(r =>
          this.$hydrateListResult(r, this.$collectionValueMeta)
        )
    );
  }

  public count(
    parameters?: FilterParameters,
    config?: AxiosRequestConfig
  ): ItemResultPromise<number> {
    return AxiosClient.get<ItemResult<number>>(
      `/${this.$metadata.controllerRoute}/count`,
      this.$options(parameters, config)
    );
  }

  public save(
    item: TModel,
    parameters?: SaveParameters<TModel>,
    config?: AxiosRequestConfig
  ): ItemResultPromise<TModel> {
    const { fields, ...params } = parameters ?? new SaveParameters<TModel>();

    return AxiosClient.post(
      `/${this.$metadata.controllerRoute}/save`,
      objectToQueryString(mapToDtoFiltered(item, fields)),
      this.$options(params, config)
    ).then<AxiosItemResult<TModel>>(r =>
      this.$hydrateItemResult(r, this.$itemValueMeta)
    );
  }

  public delete(
    id: string | number,
    parameters?: DataSourceParameters,
    config?: AxiosRequestConfig
  ): ItemResultPromise<TModel> {
    return AxiosClient.post(
      `/${this.$metadata.controllerRoute}/delete/${id}`,
      null,
      this.$options(parameters, config)
    ).then<AxiosItemResult<TModel>>(r =>
      this.$hydrateItemResult(r, this.$itemValueMeta)
    );
  }

  /** Value metadata for handling ItemResult returns from the standard API endpoints. */
  private $itemValueMeta = Object.freeze(<ModelValue>{
    name: "object",
    displayName: "",
    type: "model",
    role: "value",
    typeDef: this.$metadata
  });

  /** Value metadata for handling ListResult returns from the standard API endpoints. */
  private $collectionValueMeta = Object.freeze(<CollectionValue>{
    name: "list",
    displayName: "",
    type: "collection",
    role: "value",
    itemType: this.$itemValueMeta
  });
}

export abstract class ServiceApiClient<TMeta extends Service> extends ApiClient<
  TMeta
> {}

export type TInvoker<
  TArgs extends any[],
  TReturn,
  TClient extends ApiClient<any>
> = (this: any, client: TClient, ...args: TArgs) => TReturn;

export type TArgsInvoker<TArgs, TReturn, TClient extends ApiClient<any>> = (
  this: any,
  client: TClient,
  args: TArgs
) => TReturn;

export type TCall<TArgs extends any[], TReturn> = (
  this: any,
  ...args: TArgs
) => TReturn;

type ApiStateHook<TThis> = (this: any, state: TThis) => void | Promise<any>

export abstract class ApiState<
  TArgs extends any[],
  TResult
> extends Function {
  /** The metadata of the method being called, if it was provided. */
  $metadata?: Method;

  /** True if a request is currently pending. */
  isLoading: boolean = false;

  /** True if the previous request was successful. */
  wasSuccessful: boolean | null = null;

  /** Error message returned by the previous request. */
  message: string | null = null;

  /** Whether `.result` is null or not.
   * Using this prop to check for a result avoids a subscription 
   * against the whole result object, which will change each time the method is called.
   */
  hasResult: boolean = false;

  private _concurrencyMode: ApiCallerConcurrency = "disallow";

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
   * "debounce" - if a request is made while one is outstanding, enqueue it to start after the outstanding
   * request is done. If another request is made while one is already enqueued, the enqueued request is abandoned
   * and replaced by the last request that was made.
   *
   * "disallow" - throw an error.
   *
   * "allow" - permit the second request to be made. The ultimate state of the state fields may not be representative of the last request made.
   */
  get concurrencyMode() {
    return this._concurrencyMode;
  }
  set concurrencyMode(val: ApiCallerConcurrency) {
    this.setConcurrency(val);
  }

  // Undefined initially to prevent unneeded reactivity
  private _cancelToken: CancelTokenSource | undefined;

  // Frozen to prevent unneeded reactivity.
  private _callbacks: {
    onFulfilled: Array<ApiStateHook<any>>;
    onRejected: Array<ApiStateHook<any>>;
  } = Object.freeze({ onFulfilled: [], onRejected: [] });

  /**
   * Attach a callback to be invoked when the request to this endpoint succeeds.
   * @param callback A callback to be called when a request to this endpoint succeeds.
   */
  onFulfilled(callback: ApiStateHook<any>): this {
    if (!this._callbacks.onFulfilled.includes(callback)) {
      this._callbacks.onFulfilled.push(callback);
    }
    return this;
  }

  /**
   * Attach a callback to be invoked when the request to this endpoint fails.
   * @param callback A callback to be called when a request to this endpoint fails.
   */
  onRejected(callback: ApiStateHook<any>): this {
    if (!this._callbacks.onRejected.includes(callback)) {
      this._callbacks.onRejected.push(callback);
    }
    return this;
  }

  protected abstract setResponseProps(data: ApiResult): void;

  public invoke!: this;

  private _debounceSignal: {
    promise: Promise<void>;
    resolve: () => void;
    reject: () => void;
  } | null = null;

  protected async _invokeInternal(thisArg: any, callInvoker: () => any) {
    if (this.isLoading) {
      switch (this._concurrencyMode) {
        case "disallow":
          throw Error(
            `Request is already pending for invoker ${this.invoker.toString()}`
          );
          break;

        case "cancel":
          this.cancel();
          break;

        case "debounce":
          // If there's already a pending debounced request,
          // reject it, and then create a new promise and await a successful signal.
          if (this._debounceSignal) {
            this._debounceSignal.reject();
          }

          const signal: any = {};
          signal.promise = new Promise((resolve, reject) => {
            signal.resolve = resolve;
            signal.reject = reject;
          });
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
    this.isLoading = true;

    // Inject a cancellation token into the request.
    try {
      const token = ((this
        .apiClient as any)._nextCancelToken = axios.CancelToken.source());
      this._cancelToken = token;
      const promise = callInvoker();

      if (!promise) {
        this.isLoading = false;
        return void 0;
      }

      const resp = await promise;

      const data = resp.data;
      delete this._cancelToken;

      this.setResponseProps(data);

      const onFulfilled = this._callbacks.onFulfilled;
      for (let i = 0; i < onFulfilled.length; i++) {
        const cb = onFulfilled[i];
        const cbResult = cb.apply(thisArg, [this]);
        if (cbResult instanceof Promise) {
          await cbResult;
        }
      }

      this.isLoading = false;

      // We have to maintain the shape of the promise of the stateless invoke method.
      // This means we can't re-shape ourselves into a Promise<ApiState<T>> with `return fn` here.
      // The reason for this is that we can't change the return type of TCall while maintaining
      // the param signature (unless we required a full, explicit type annotation as a type parameter,
      // but this would make the usability of apiCallers very unpleasant.)
      // We could do this easily with https://github.com/Microsoft/TypeScript/issues/5453,
      // but changing the implementation would be a significant breaking change by then.
      return resp;
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
        var error = thrown as AxiosError | ApiResult | Error | string;
      }
      
      try {
        delete this._cancelToken;
        this.wasSuccessful = false;
        const result = typeof error === "object" && "response" in error 
          ? error.response as
            | AxiosResponse<ListResult<TResult> 
            | ItemResult<TResult>>
            | undefined
          : undefined;
        if (result && typeof result.data === "object") {
          this.setResponseProps(result.data);
        } else {
          this.message = getMessageForError(error)
        }

        const onRejected = this._callbacks.onRejected;
        for (let i = 0; i < onRejected.length; i++) {
          const cb = onRejected[i];
          const cbResult = cb.apply(thisArg, [this]);
          if (cbResult instanceof Promise) {
            await cbResult;
          }
        }
      } finally {
        this.isLoading = false;
      }

      throw error;
    } finally {
      delete (this.apiClient as any)._nextCancelToken;

      if (this._debounceSignal) {
        this._debounceSignal.resolve();
        this._debounceSignal = null;
      }
    }
  }

  protected _makeReactive() {
    // Make properties reactive. Works around https://github.com/vuejs/vue/issues/6648

    // Use Object.keys to mock the behavior of
    // https://github.com/vuejs/vue/blob/4c7a87e2ef9c76b5b75d85102662a27165a23ec7/src/core/observer/index.js#L61
    const keys = Object.keys(this);
    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];

      // @ts-ignore - Ignore indexer on type without indexer signature.
      const value = this[key];

      // Don't define sealed object properties (e.g. this._callbacks)
      if (
        value == null ||
        typeof value !== "object" ||
        !Object.isSealed(value)
      ) {
        Vue.util.defineReactive(this, key, value);
      }
    }
  }

  constructor(
    protected readonly apiClient: ApiClient<any>,
    private readonly invoker: any
    // TInvoker<
    //   TArgs,
    //   ApiResultPromise<TResult> | undefined | void,
    //   TClient
    // >
  ) {
    super();

    // Create our invoker function that will ultimately be our instance object.
    const invokeFunc: TCall<
      TArgs,
      ApiResultPromise<TResult>
    > = function invokeFunc(this: any, ...args: TArgs) {
      return invoke._invokeInternal(this, () => {
        return (invoker as any).apply(this, [apiClient, ...args]);
      });
    } as TCall<TArgs, ApiResultPromise<TResult>>;

    // Copy all properties from the class to the function.
    const invoke = Object.assign(invokeFunc, this);
    invoke.invoke = invoke;

    Object.setPrototypeOf(invoke, new.target.prototype);
    return invoke;
  }
}

export class ItemApiState<
  TArgs extends any[],
  TResult
> extends ApiState<TArgs, TResult> {
  /** The metadata of the method being called, if it was provided. */
  $metadata?: ItemMethod;

  /** Validation issues returned by the previous request. */
  validationIssues: ValidationIssue[] | null = null;

  /** Principal data returned by the previous request. */
  result: TResult | null = null;

  constructor(
    apiClient: ApiClient<any>,
    invoker: TInvoker<
      TArgs,
      ApiResultPromise<TResult> | undefined | void,
      ApiClient<any>
    >
  ) {
    super(apiClient, invoker);
    this._makeReactive();
  }

  protected setResponseProps(data: ItemResult<TResult>) {
    this.wasSuccessful = data.wasSuccessful;
    this.message = data.message || null;

    if ("validationIssues" in data) {
      this.validationIssues = data.validationIssues || null;
    } else {
      this.validationIssues = null;
    }
    if ("object" in data) {
      this.result = data.object ?? null;
    } else {
      this.result = null;
    }
    this.hasResult = this.result != null;
  }
}

export class ItemApiStateWithArgs<
  TArgs extends any[],
  TArgsObj,
  TResult
> extends ItemApiState<TArgs, TResult> {
  /** Values that will be used as arguments if the method is invoked with `this.invokeWithArgs()`. */
  public args: TArgsObj = this.argsFactory();

  /** Invoke the method. If `args` is not provided, the values in `this.args` will be used for the method's parameters. */
  public invokeWithArgs(args: TArgsObj = this.args): InvokerReturnType<"item", TResult> {
    args = { ...args }; // Copy args so that if we're debouncing,
    // the args at the point in time at which invokeWithArgs() was
    // called will be used, rather than the state at the time when the actual API call gets made.
    return this._invokeInternal(this, () =>
      this.argsInvoker.apply(this, [this.apiClient, args])
    );
  }

  /** Replace `this.args` with a new, blank object containing default values (typically nulls) */
  public resetArgs() {
    this.args = this.argsFactory();
  }

  constructor(
    apiClient: ApiClient<any>,
    invoker: TInvoker<TArgs, ItemResultPromise<TResult>, ApiClient<any>>,
    private argsFactory: () => TArgsObj,
    private argsInvoker: TArgsInvoker<
      TArgsObj,
      ItemResultPromise<TResult>,
      ApiClient<any>
    >
  ) {
    super(apiClient, invoker);
    this._makeReactive();
  }
}

export class ListApiState<
  TArgs extends any[],
  TResult
> extends ApiState<TArgs, TResult> {
  /** The metadata of the method being called, if it was provided. */
  $metadata?: ListMethod;

  /** Page number returned by the previous request. */
  page: number | null = null;
  /** Page size returned by the previous request. */
  pageSize: number | null = null;
  /** Page count returned by the previous request. */
  pageCount: number | null = null;
  /** Total Count returned by the previous request. */
  totalCount: number | null = null;

  /** Principal data returned by the previous request. */
  result: TResult[] | null = null;

  constructor(
    apiClient: ApiClient<any>,
    invoker: TInvoker<
      TArgs,
      ApiResultPromise<TResult> | undefined | void,
      ApiClient<any>
    >
  ) {
    super(apiClient, invoker);
    this._makeReactive();
  }

  protected setResponseProps(data: ListResult<TResult>) {
    this.wasSuccessful = data.wasSuccessful;
    this.message = data.message || null;

    this.page = data.page;
    this.pageSize = data.pageSize;
    this.pageCount = data.pageCount;
    this.totalCount = data.totalCount;

    if ("list" in data) {
      this.result = data.list || [];
    } else {
      this.result = null;
    }
    this.hasResult = this.result != null;
  }
}

export class ListApiStateWithArgs<
  TArgs extends any[],
  TArgsObj,
  TResult
> extends ListApiState<TArgs, TResult> {
  /** Values that will be used as arguments if the method is invoked with `this.invokeWithArgs()`. */
  public args: TArgsObj = this.argsFactory();

  /** Invoke the method. If `args` is not provided, the values in `this.args` will be used for the method's parameters. */
  public invokeWithArgs(args: TArgsObj = this.args): InvokerReturnType<"list", TResult> {
    args = { ...args }; // Copy args so that if we're debouncing,
    // the args at the point in time at which invokeWithArgs() was
    // called will be used, rather than the state at the time when the actual API call gets made.
    return this._invokeInternal(this, () =>
      this.argsInvoker.apply(this, [this.apiClient, args])
    );
  }

  /** Replace `this.args` with a new, blank object containing default values (typically nulls) */
  public resetArgs() {
    this.args = this.argsFactory();
  }

  constructor(
    apiClient: ApiClient<any>,
    invoker: TInvoker<TArgs, ListResultPromise<TResult>, ApiClient<any>>,
    private argsFactory: () => TArgsObj,
    private argsInvoker: TArgsInvoker<
      TArgsObj,
      ListResultPromise<TResult>,
      ApiClient<any>
    >
  ) {
    super(apiClient, invoker);
    this._makeReactive();
  }
}

export type AnyArgCaller<
  TArgs extends any[] = any[],
  TArgsObj = any,
  TResult = any
> = 
  | ListApiStateWithArgs<TArgs,TArgsObj,TResult> 
  | ItemApiStateWithArgs<TArgs,TArgsObj,TResult>

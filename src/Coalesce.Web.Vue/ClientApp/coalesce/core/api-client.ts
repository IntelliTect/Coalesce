
import { IHaveMetadata, ModelType, hydrateMetadata } from './metadata'

import axios, { AxiosPromise, AxiosResponse } from 'axios';
import * as qs from 'qs';
import { mapToDto } from '../index';




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


export const AxiosClient = axios.create();
AxiosClient.defaults.baseURL = 'http://localhost:11202/api/';
AxiosClient.defaults.withCredentials = true;

export class ApiClient<T extends IHaveMetadata = IHaveMetadata> {
    
    constructor(public readonly metadata: ModelType) {
        
    }

    public get(id: any, parameters?: DataSourceParameters) {
        return AxiosClient
            .get(`/${this.metadata.controllerRoute}/get/${id}`, {params: this.objectify(parameters)})
            .then(this.hydrateItemResult.bind(this))
    }
    
    public list(parameters?: ListParameters) {
        return AxiosClient
            .get(`/${this.metadata.controllerRoute}/list`, {params: this.objectify(parameters)})
            .then(this.hydrateListResult.bind(this))
    }
    
    public count(parameters?: FilterParameters) {
        return AxiosClient
            .get(`/${this.metadata.controllerRoute}/count`, {params: this.objectify(parameters)})
    }
    
    public save(item: T, parameters?: DataSourceParameters) {
        return AxiosClient
            .post(
                `/${this.metadata.controllerRoute}/save`,
                qs.stringify(mapToDto(item)),
                { params: this.objectify(parameters) }
            )
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

    private hydrateItemResult<T extends IHaveMetadata>(value: AxiosResponse<ItemResult<T>>) {
        const object = value.data.object;
        if (object) {
            hydrateMetadata(object, this.metadata)
        }
        return value;
    }

    private hydrateListResult<T extends IHaveMetadata>(value: AxiosResponse<ListResult<T>>) {
        const list = value.data.list;
        if (Array.isArray(list)) {
            list.forEach(item => hydrateMetadata(item, this.metadata))
        }
        return value;
    }
}
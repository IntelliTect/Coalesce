/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.Company.ts" />

// Knockout List View Model for: Company
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum CompanyDataSources {
            Default,
        }
    export class CompanyList extends BaseListViewModel<CompanyList, ViewModels.Company> {
        protected modelName = "Company";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/Company";
        public modelKeyName = "companyId";
        public dataSources = CompanyDataSources;
        public itemClass = ViewModels.Company;

        public query: {
            where?: string;
            companyId?:number;
            name?:String;
            address1?:String;
            address2?:String;
            city?:String;
            state?:String;
            zipCode?:String;
            altName?:String;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: CompanyDataSources = CompanyDataSources.Default;

        // Valid values
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Company(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations
        }
    }

    export namespace CompanyList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
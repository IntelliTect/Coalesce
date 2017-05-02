/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.CaseProduct.ts" />

// Knockout List View Model for: CaseProduct
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum CaseProductDataSources {
            Default,
        }
    export class CaseProductList extends BaseListViewModel<CaseProductList, ViewModels.CaseProduct> {
        protected modelName = "CaseProduct";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/CaseProduct";
        public modelKeyName = "caseProductId";
        public dataSources = CaseProductDataSources;
        public itemClass = ViewModels.CaseProduct;

        public query: {
            where?: string;
            caseProductId?:number;
            caseId?:number;
            productId?:number;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: CaseProductDataSources = CaseProductDataSources.Default;

        // Valid values
        public caseValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCaseValidValues: (callback: any) => void;
        public productValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadProductValidValues: (callback: any) => void;
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.CaseProduct(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations
        }
    }

    export namespace CaseProductList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
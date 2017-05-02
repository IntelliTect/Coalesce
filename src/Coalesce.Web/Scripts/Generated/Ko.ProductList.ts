/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.Product.ts" />

// Knockout List View Model for: Product
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum ProductDataSources {
            Default,
        }
    export class ProductList extends BaseListViewModel<ProductList, ViewModels.Product> {
        protected modelName = "Product";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/Product";
        public modelKeyName = "productId";
        public dataSources = ProductDataSources;
        public itemClass = ViewModels.Product;

        public query: {
            where?: string;
            productId?:number;
            name?:String;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: ProductDataSources = ProductDataSources.Default;

        // Valid values
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Product(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations
        }
    }

    export namespace ProductList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
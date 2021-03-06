
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ListViewModels {
    
    export namespace ProductDataSources {
        export class Default extends Coalesce.DataSource<ViewModels.Product> { }
    }
    
    export class ProductList extends Coalesce.BaseListViewModel<ViewModels.Product> {
        public readonly modelName: string = "Product";
        public readonly apiController: string = "/Product";
        public modelKeyName: string = "productId";
        public itemClass: new () => ViewModels.Product = ViewModels.Product;
        
        public filter: {
            productId?: string;
            name?: string;
            uniqueId?: string;
        } | null = null;
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof ProductDataSources = ProductDataSources;
        
        /** The data source on the server to use when retrieving objects. Valid values are in this.dataSources. */
        public dataSource: Coalesce.DataSource<ViewModels.Product> = new this.dataSources.Default();
        
        /** Configuration for all instances of ProductList. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<ProductList, ViewModels.Product>(Coalesce.GlobalConfiguration.listViewModel);
        
        /** Configuration for this ProductList instance. */
        public coalesceConfig: Coalesce.ListViewModelConfiguration<ProductList, ViewModels.Product>
            = new Coalesce.ListViewModelConfiguration<ProductList, ViewModels.Product>(ProductList.coalesceConfig);
        
        
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Product(newItem, parent);
        
        constructor() {
            super();
        }
    }
}

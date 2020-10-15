
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ListViewModels {
    
    export namespace StandaloneReadonlyDataSources {
        export class Default extends Coalesce.DataSource<ViewModels.StandaloneReadonly> { }
        export class DefaultSource extends Coalesce.DataSource<ViewModels.StandaloneReadonly> {
        }
    }
    
    export class StandaloneReadonlyList extends Coalesce.BaseListViewModel<ViewModels.StandaloneReadonly> {
        public readonly modelName: string = "StandaloneReadonly";
        public readonly apiController: string = "/StandaloneReadonly";
        public modelKeyName: string = "id";
        public itemClass: new () => ViewModels.StandaloneReadonly = ViewModels.StandaloneReadonly;
        
        public filter: {
            id?: string;
            name?: string;
            description?: string;
        } | null = null;
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof StandaloneReadonlyDataSources = StandaloneReadonlyDataSources;
        
        /** The data source on the server to use when retrieving objects. Valid values are in this.dataSources. */
        public dataSource: Coalesce.DataSource<ViewModels.StandaloneReadonly> = new this.dataSources.Default();
        
        /** Configuration for all instances of StandaloneReadonlyList. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<StandaloneReadonlyList, ViewModels.StandaloneReadonly>(Coalesce.GlobalConfiguration.listViewModel);
        
        /** Configuration for this StandaloneReadonlyList instance. */
        public coalesceConfig: Coalesce.ListViewModelConfiguration<StandaloneReadonlyList, ViewModels.StandaloneReadonly>
            = new Coalesce.ListViewModelConfiguration<StandaloneReadonlyList, ViewModels.StandaloneReadonly>(StandaloneReadonlyList.coalesceConfig);
        
        
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.StandaloneReadonly(newItem, parent);
        
        constructor() {
            super();
        }
    }
}

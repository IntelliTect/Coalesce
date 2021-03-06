
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ListViewModels {
    
    export namespace LogDataSources {
        export class Default extends Coalesce.DataSource<ViewModels.Log> { }
    }
    
    export class LogList extends Coalesce.BaseListViewModel<ViewModels.Log> {
        public readonly modelName: string = "Log";
        public readonly apiController: string = "/Log";
        public modelKeyName: string = "logId";
        public itemClass: new () => ViewModels.Log = ViewModels.Log;
        
        public filter: {
            logId?: string;
            level?: string;
            message?: string;
        } | null = null;
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof LogDataSources = LogDataSources;
        
        /** The data source on the server to use when retrieving objects. Valid values are in this.dataSources. */
        public dataSource: Coalesce.DataSource<ViewModels.Log> = new this.dataSources.Default();
        
        /** Configuration for all instances of LogList. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<LogList, ViewModels.Log>(Coalesce.GlobalConfiguration.listViewModel);
        
        /** Configuration for this LogList instance. */
        public coalesceConfig: Coalesce.ListViewModelConfiguration<LogList, ViewModels.Log>
            = new Coalesce.ListViewModelConfiguration<LogList, ViewModels.Log>(LogList.coalesceConfig);
        
        
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Log(newItem, parent);
        
        constructor() {
            super();
        }
    }
}

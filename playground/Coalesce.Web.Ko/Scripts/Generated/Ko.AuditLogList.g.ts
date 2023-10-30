
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ListViewModels {
    
    export namespace AuditLogDataSources {
        export class Default extends Coalesce.DataSource<ViewModels.AuditLog> { }
    }
    
    export class AuditLogList extends Coalesce.BaseListViewModel<ViewModels.AuditLog> {
        public readonly modelName: string = "AuditLog";
        public readonly apiController: string = "/AuditLog";
        public modelKeyName: string = "id";
        public itemClass: new () => ViewModels.AuditLog = ViewModels.AuditLog;
        
        public filter: {
            message?: string;
            userId?: string;
            id?: string;
            type?: string;
            keyValue?: string;
            state?: string;
            date?: string;
            clientIp?: string;
            referrer?: string;
            endpoint?: string;
        } | null = null;
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof AuditLogDataSources = AuditLogDataSources;
        
        /** The data source on the server to use when retrieving objects. Valid values are in this.dataSources. */
        public dataSource: Coalesce.DataSource<ViewModels.AuditLog> = new this.dataSources.Default();
        
        /** Configuration for all instances of AuditLogList. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<AuditLogList, ViewModels.AuditLog>(Coalesce.GlobalConfiguration.listViewModel);
        
        /** Configuration for this AuditLogList instance. */
        public coalesceConfig: Coalesce.ListViewModelConfiguration<AuditLogList, ViewModels.AuditLog>
            = new Coalesce.ListViewModelConfiguration<AuditLogList, ViewModels.AuditLog>(AuditLogList.coalesceConfig);
        
        
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.AuditLog(newItem, parent);
        
        constructor() {
            super();
        }
    }
}

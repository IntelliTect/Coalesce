
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ListViewModels {
    
    export namespace AuditLogPropertyDataSources {
        export class Default extends Coalesce.DataSource<ViewModels.AuditLogProperty> { }
    }
    
    export class AuditLogPropertyList extends Coalesce.BaseListViewModel<ViewModels.AuditLogProperty> {
        public readonly modelName: string = "AuditLogProperty";
        public readonly apiController: string = "/AuditLogProperty";
        public modelKeyName: string = "id";
        public itemClass: new () => ViewModels.AuditLogProperty = ViewModels.AuditLogProperty;
        
        public filter: {
            id?: string;
            parentId?: string;
            propertyName?: string;
            oldValue?: string;
            newValue?: string;
        } | null = null;
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof AuditLogPropertyDataSources = AuditLogPropertyDataSources;
        
        /** The data source on the server to use when retrieving objects. Valid values are in this.dataSources. */
        public dataSource: Coalesce.DataSource<ViewModels.AuditLogProperty> = new this.dataSources.Default();
        
        /** Configuration for all instances of AuditLogPropertyList. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<AuditLogPropertyList, ViewModels.AuditLogProperty>(Coalesce.GlobalConfiguration.listViewModel);
        
        /** Configuration for this AuditLogPropertyList instance. */
        public coalesceConfig: Coalesce.ListViewModelConfiguration<AuditLogPropertyList, ViewModels.AuditLogProperty>
            = new Coalesce.ListViewModelConfiguration<AuditLogPropertyList, ViewModels.AuditLogProperty>(AuditLogPropertyList.coalesceConfig);
        
        
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.AuditLogProperty(newItem, parent);
        
        constructor() {
            super();
        }
    }
}

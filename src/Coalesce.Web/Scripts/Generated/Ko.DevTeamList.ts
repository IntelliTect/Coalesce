/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.DevTeam.ts" />

// Knockout List View Model for: DevTeam
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum DevTeamDataSources {
            Default,
        }
    export class DevTeamList extends BaseListViewModel<DevTeamList, ViewModels.DevTeam> {
        protected modelName = "DevTeam";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/DevTeam";
        public modelKeyName = "devTeamId";
        public dataSources = DevTeamDataSources;
        public itemClass = ViewModels.DevTeam;

        public query: {
            where?: string;
            devTeamId?:number;
            name?:String;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: DevTeamDataSources = DevTeamDataSources.Default;

        // Valid values
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.DevTeam(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations
        }
    }

    export namespace DevTeamList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
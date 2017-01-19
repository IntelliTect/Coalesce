/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="./Ko.CaseDto.ts" />

// Knockout List View Model for: CaseDto
// Auto Generated Knockout List Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum CaseDtoDataSources {
            Default,
            GetAllOpenCases,
        }
    export class CaseDtoList extends BaseListViewModel<CaseDtoList, ViewModels.CaseDto> {
        protected modelName = "CaseDto";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected apiUrlBase = "api/CaseDto";
        public dataSources = CaseDtoDataSources;

        public query: {
            where?: string;
            caseKey?:number;
            title?:String;
            description?:String;
            openedAt?:any;
            assignedToId?:number;
            reportedById?:number;
            severity?:String;
            status?:number;
            devTeamAssignedId?:number;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: CaseDtoDataSources = CaseDtoDataSources.Default;

        // Valid values
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.CaseDto(newItem, parent);

        constructor() {
            super();
            var self = this; 

    // Method Implementations
        }
    }

    export namespace CaseDtoList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}
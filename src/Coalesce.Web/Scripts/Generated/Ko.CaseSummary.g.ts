
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type CaseSummary
    export class CaseSummary
    {
        public myId: any = 0;

        // Observables
        public caseSummaryId: KnockoutObservable<number | null> = ko.observable(null);
        public openCases: KnockoutObservable<number | null> = ko.observable(null);
        public caseCount: KnockoutObservable<number | null> = ko.observable(null);
        public closeCases: KnockoutObservable<number | null> = ko.observable(null);
        public description: KnockoutObservable<string | null> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public parent: any;
        public parentCollection: any;

        public loadFromDto = (data: any) => {
            if (!data) return;
            // Set the ID
            this.myId = data.caseSummaryId;

            // Load the properties.
            this.caseSummaryId(data.caseSummaryId);
            this.openCases(data.openCases);
            this.caseCount(data.caseCount);
            this.closeCases(data.closeCases);
            this.description(data.description);

        };

                /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            dto.caseSummaryId = this.caseSummaryId();
            
            dto.openCases = this.openCases();
            dto.caseCount = this.caseCount();
            dto.closeCases = this.closeCases();
            dto.description = this.description();
            
            return dto;
        }


        constructor(newItem?: any, parent?: any){
            this.parent = parent;
            // Load the object

            if (newItem) {
                this.loadFromDto(newItem);
            }
        }
    }
}

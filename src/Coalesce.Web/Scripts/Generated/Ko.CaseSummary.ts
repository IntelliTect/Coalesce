/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />

module ViewModels {
    // *** External Type CaseSummary
    export class CaseSummary
    {
        public myId: any = 0;

        // Observables
		public caseSummaryId: KnockoutObservable<number> = ko.observable(null);
		public openCases: KnockoutObservable<number> = ko.observable(null);
		public caseCount: KnockoutObservable<number> = ko.observable(null);
		public closeCases: KnockoutObservable<number> = ko.observable(null);
		public description: KnockoutObservable<string> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public loadFromDto: (data: any) => void;
        public parent: any;


        constructor(newItem?: any, parent?: any){
            var self = this;
            self.parent = parent;
            // Load the object
			self.loadFromDto = function(data: any) {
				if (!data) return;
                // Set the ID
                self.myId = data.caseSummaryId;

                // Load the properties.
                self.caseSummaryId(data.caseSummaryId);
                self.openCases(data.openCases);
                self.caseCount(data.caseCount);
                self.closeCases(data.closeCases);
                self.description(data.description);
                
            };

            if (newItem) {
                self.loadFromDto(newItem);
            }
        }
    }
}

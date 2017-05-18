
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type PersonStats
    export class PersonStats
    {
        public myId: any = 0;

        // Observables
		public personStatsId: KnockoutObservable<number> = ko.observable(null);
		public height: KnockoutObservable<number> = ko.observable(null);
		public weight: KnockoutObservable<number> = ko.observable(null);
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
                self.myId = data.personStatsId;

                // Load the properties.
                self.personStatsId(data.personStatsId);
                self.height(data.height);
                self.weight(data.weight);
                
            };

            if (newItem) {
                self.loadFromDto(newItem);
            }
        }
    }
}

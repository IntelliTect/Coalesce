
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type PersonStats
    export class PersonStats
    {

        // Observables
		public height: KnockoutObservable<number> = ko.observable(null);
		public weight: KnockoutObservable<number> = ko.observable(null);
		public name: KnockoutObservable<string> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public loadFromDto: (data: any) => void;
        public parent: any;
        public parentCollection: any;


        constructor(newItem?: any, parent?: any){
            var self = this;
            self.parent = parent;
            // Load the object
			self.loadFromDto = function(data: any) {
				if (!data) return;

                // Load the properties.
                self.height(data.height);
                self.weight(data.weight);
                self.name(data.name);
                
            };

            if (newItem) {
                self.loadFromDto(newItem);
            }
        }
    }
}

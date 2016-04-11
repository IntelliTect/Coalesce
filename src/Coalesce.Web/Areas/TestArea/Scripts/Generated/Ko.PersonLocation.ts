/// <reference path="../../../../typings/tsd.d.ts" />
/// <reference path="../../../../scripts/Intellitect/intellitect.utilities.ts" />
/// <reference path="../../../../scripts/Intellitect/intellitect.ko.utilities.ts" />

module TestArea.ViewModels {
    // *** External Type PersonLocation
    export class PersonLocation
    {
        // Observables
		public latitude: KnockoutObservable<number> = ko.observable(null);
		public longitude: KnockoutObservable<number> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public loadFromDto: (data: any) => void;
        public parent: any;


        constructor(newItem?: any, parent?: any){
            var self = this;
            self.parent = parent;
            // Load the object
			self.loadFromDto = function(data: any) {
				if (!data) return;

                // Load the properties.
                self.latitude(data.Latitude);
                self.longitude(data.Longitude);
                
            };

            if (newItem) {
                self.loadFromDto(newItem);
            }
        }
    }
}

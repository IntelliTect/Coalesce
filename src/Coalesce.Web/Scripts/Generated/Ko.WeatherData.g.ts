
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type WeatherData
    export class WeatherData
    {

        // Observables
		public tempFahrenheit: KnockoutObservable<number> = ko.observable(null);
		public humidity: KnockoutObservable<number> = ko.observable(null);
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
                self.tempFahrenheit(data.tempFahrenheit);
                self.humidity(data.humidity);
                
            };

            if (newItem) {
                self.loadFromDto(newItem);
            }
        }
    }
}

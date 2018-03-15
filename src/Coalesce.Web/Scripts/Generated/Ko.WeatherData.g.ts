
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ViewModels {
    /** External Type WeatherData */
    export class WeatherData {
        
        // Observables
        public tempFahrenheit: KnockoutObservable<number | null> = ko.observable(null);
        public humidity: KnockoutObservable<number | null> = ko.observable(null);
        public location: KnockoutObservable<ViewModels.Location | null> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public parent: any;
        public parentCollection: any;
        
        
        /** 
            Load the object from the DTO.
            @param data: The incoming data object to load.
        */
        public loadFromDto = (data: any) => {
            if (!data) return;
            
            // Load the properties.
            this.tempFahrenheit(data.tempFahrenheit);
            this.humidity(data.humidity);
            if (!this.location()){
                this.location(new Location(data.location, this));
            } else {
                this.location()!.loadFromDto(data.location);
            }
            
        };
        
        
        /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            
            dto.tempFahrenheit = this.tempFahrenheit();
            dto.humidity = this.humidity();
            
            return dto;
        }
        
        constructor(newItem?: any, parent?: any) {
            this.parent = parent;
            // Load the object
            
            if (newItem) {
                this.loadFromDto(newItem);
            }
        }
    }
}

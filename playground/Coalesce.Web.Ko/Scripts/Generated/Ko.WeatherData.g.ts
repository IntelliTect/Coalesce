
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ViewModels {
    
    /** External Type WeatherData */
    export class WeatherData {
        public parent: any;
        public parentCollection: any;
        
        // Observables
        public tempFahrenheit: KnockoutObservable<number | null> = ko.observable(null);
        public humidity: KnockoutObservable<number | null> = ko.observable(null);
        public location: KnockoutObservable<ViewModels.Location | null> = ko.observable(null);
        
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
            dto.location = this.location()?.saveToDto();
            
            return dto;
        }
        
        constructor(newItem?: any, parent?: any) {
            this.parent = parent;
            
            if (newItem) {
                this.loadFromDto(newItem);
            }
        }
    }
}


/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ViewModels {
    
    /** External Type PersonLocation */
    export class PersonLocation {
        public parent: any;
        public parentCollection: any;
        
        // Observables
        public latitude: KnockoutObservable<number | null> = ko.observable(null);
        public longitude: KnockoutObservable<number | null> = ko.observable(null);
        
        /** 
            Load the object from the DTO.
            @param data: The incoming data object to load.
        */
        public loadFromDto = (data: any) => {
            if (!data) return;
            
            // Load the properties.
            this.latitude(data.latitude);
            this.longitude(data.longitude);
            
        };
        
        /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            
            dto.latitude = this.latitude();
            dto.longitude = this.longitude();
            
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

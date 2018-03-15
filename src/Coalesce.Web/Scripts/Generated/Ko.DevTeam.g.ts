
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ViewModels {
    
    /** External Type DevTeam */
    export class DevTeam {
        public myId: any = 0;
        
        // Observables
        public devTeamId: KnockoutObservable<number | null> = ko.observable(null);
        public name: KnockoutObservable<string | null> = ko.observable(null);
        public parent: any;
        public parentCollection: any;
        
        
        /** 
            Load the object from the DTO.
            @param data: The incoming data object to load.
        */
        public loadFromDto = (data: any) => {
            if (!data) return;
            // Set the ID
            this.myId = data.devTeamId;
            
            // Load the properties.
            this.devTeamId(data.devTeamId);
            this.name(data.name);
            
        };
        
        
        /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            dto.devTeamId = this.devTeamId();
            
            dto.name = this.name();
            
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

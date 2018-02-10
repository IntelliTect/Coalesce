
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type Location
    export class Location
    {

        // Observables
        public city: KnockoutObservable<string> = ko.observable(null);
        public state: KnockoutObservable<string> = ko.observable(null);
        public zip: KnockoutObservable<string> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public parent: any;
        public parentCollection: any;

        public loadFromDto = (data: any) => {
            if (!data) return;

            // Load the properties.
            this.city(data.city);
            this.state(data.state);
            this.zip(data.zip);

        };

                /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            
            dto.city = this.city();
            dto.state = this.state();
            dto.zip = this.zip();
            
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

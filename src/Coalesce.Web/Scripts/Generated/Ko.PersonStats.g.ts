
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
        public parent: any;
        public parentCollection: any;

        public loadFromDto = (data: any) => {
            if (!data) return;

            // Load the properties.
            this.height(data.height);
            this.weight(data.weight);
            this.name(data.name);

        };

                /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            
            dto.height = this.height();
            dto.weight = this.weight();
            dto.name = this.name();
            
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

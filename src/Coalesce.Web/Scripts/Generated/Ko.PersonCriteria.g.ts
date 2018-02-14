
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type PersonCriteria
    export class PersonCriteria
    {

        // Observables
        public name: KnockoutObservable<string | null> = ko.observable(null);
        public birthdayMonth: KnockoutObservable<number | null> = ko.observable(null);
        public emailDomain: KnockoutObservable<string | null> = ko.observable(null);
        // Loads this object from a data transfer object received from the server.
        public parent: any;
        public parentCollection: any;

        public loadFromDto = (data: any) => {
            if (!data) return;

            // Load the properties.
            this.name(data.name);
            this.birthdayMonth(data.birthdayMonth);
            this.emailDomain(data.emailDomain);

        };

                /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            
            dto.name = this.name();
            dto.birthdayMonth = this.birthdayMonth();
            dto.emailDomain = this.emailDomain();
            
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

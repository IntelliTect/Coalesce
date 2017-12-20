
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    // *** External Type DevTeam
    export class DevTeam
    {
        public myId: any = 0;

        // Observables
		public devTeamId: KnockoutObservable<number> = ko.observable(null);
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
                // Set the ID
                self.myId = data.devTeamId;

                // Load the properties.
                self.devTeamId(data.devTeamId);
                self.name(data.name);
                
            };

            if (newItem) {
                self.loadFromDto(newItem);
            }
        }
    }
}

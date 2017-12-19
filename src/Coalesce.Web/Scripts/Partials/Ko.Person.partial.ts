/// <reference path="../viewmodels.generated.d.ts" />



module ViewModels {
	export class Person extends PersonPartial {

        constructor(newItem?: object, parent?: Coalesce.BaseViewModel | ListViewModels.PersonList) {
            super(newItem, parent);

            // Not really doing anything with this - just making sure it works.
        }
    }
}
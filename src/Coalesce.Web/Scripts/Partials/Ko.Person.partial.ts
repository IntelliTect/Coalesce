
/// <reference path="../coalesce.dependencies.d.ts" />

module ViewModels {
    export class Person extends PersonPartial {
        
        constructor(newItem?: object, parent?: Coalesce.BaseViewModel | ListViewModels.PersonList) {
            super(newItem, parent);
            
        }
    }
}

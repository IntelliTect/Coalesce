/// <reference path="../typings/tsd.d.ts" />
/// <reference path="./Intellitect.References.d.ts" />

var model = new ViewModels.Person();
model.load(1);
model.isSavingAutomatically = false;
ko.applyBindings(model);